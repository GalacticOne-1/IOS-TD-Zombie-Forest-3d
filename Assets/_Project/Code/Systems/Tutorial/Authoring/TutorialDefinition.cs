using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Полная кампания тутора (например FIRST_TIME_PLAYER). Строит и кэширует
    /// lookup-реестры по stepId/chapterId через проектный RegistryBase&lt;TKey,TValue&gt;
    /// (см. GameDatabase.Registries) для O(1) доступа, вместо линейного сканирования.
    ///
    /// stepId/chapterId/campaignId — RuntimeId-ассеты (TutorialStepId/TutorialChapterId/
    /// TutorialCampaignId), не строки. Обычная прогрессия по графу всегда резолвится через
    /// typed GetStep(TutorialStepId) — прямой lookup по ссылке, без похода в guid. Единственное
    /// место, где нужен guid-based поиск — восстановление после персиста: CGameStateTutorial
    /// хранит currentStepId/checkpointStepId как string (TutorialStepId.Guid), потому что
    /// ScriptableObject-ссылку нельзя напрямую положить в сохранение. Для этого случая —
    /// GetStepByGuid().
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialCampaign_",
        menuName = "Galactic1/Tutorial/Campaign")]
    public sealed class TutorialDefinition : ScriptableObject
    {
        [Tooltip("Стабильный уникальный идентификатор кампании (RuntimeId-ассет, например FIRST_TIME_PLAYER).")]
        public TutorialCampaignId campaignId;

        [Tooltip("stepId точки входа графа.")]
        public TutorialStepId entryStepId;

        public List<TutorialChapterDefinition> chapters = new();

        private sealed class StepRegistry : RegistryBase<TutorialStepId, TutorialStepDefinition>
        {
            public void Populate(List<TutorialChapterDefinition> chapters)
            {
                map.Clear();
                if (chapters == null) return;
                foreach (var chapter in chapters)
                {
                    if (chapter?.steps == null) continue;
                    foreach (var step in chapter.steps)
                    {
                        if (step == null || step.stepId == null) continue;
                        map[step.stepId] = step;
                    }
                }
            }
        }

        private sealed class ChapterRegistry : RegistryBase<TutorialChapterId, TutorialChapterDefinition>
        {
            public void Populate(List<TutorialChapterDefinition> chapters)
            {
                map.Clear();
                if (chapters == null) return;
                foreach (var chapter in chapters)
                {
                    if (chapter == null || chapter.chapterId == null) continue;
                    map[chapter.chapterId] = chapter;
                }
            }
        }

        private readonly StepRegistry _stepRegistry = new();
        private readonly ChapterRegistry _chapterRegistry = new();

        /// <summary>Guid(TutorialStepId) → step. Populate вместе с _stepRegistry.
        /// Только для GetStepByGuid — см. классовый докстринг.</summary>
        private readonly Dictionary<string, TutorialStepDefinition> _stepByGuid = new();

        private bool _cacheBuilt;

        private void OnEnable() => RebuildCache();
#if UNITY_EDITOR
        private void OnValidate() => RebuildCache();
#endif

        private void RebuildCache()
        {
            _stepRegistry.Populate(chapters);
            _chapterRegistry.Populate(chapters);

            _stepByGuid.Clear();
            if (chapters != null)
            {
                foreach (var chapter in chapters)
                {
                    if (chapter?.steps == null) continue;
                    foreach (var step in chapter.steps)
                    {
                        if (step == null || step.stepId == null) continue;
                        _stepByGuid[step.stepId.Guid] = step;
                    }
                }
            }

            _cacheBuilt = true;
        }

        /// <summary>O(1) доступ к шагу по typed-ссылке. Единственный способ, которым
        /// живая прогрессия графа должна получать TutorialStepDefinition — никогда по
        /// индексу списка и никогда через guid-строку (см. GetStepByGuid).</summary>
        public TutorialStepDefinition GetStep(TutorialStepId stepId)
        {
            if (!_cacheBuilt) RebuildCache();
            return stepId != null && _stepRegistry.TryGet(stepId, out var step) ? step : null;
        }

        /// <summary>Резолв шага из персистентного guid (CGameStateTutorial.currentStepId/
        /// checkpointStepId). Использовать ТОЛЬКО на границе save/restore — обычная
        /// прогрессия по графу обязана идти через GetStep(TutorialStepId).</summary>
        public TutorialStepDefinition GetStepByGuid(string guid)
        {
            if (!_cacheBuilt) RebuildCache();
            return !string.IsNullOrEmpty(guid) && _stepByGuid.TryGetValue(guid, out var step) ? step : null;
        }

        public TutorialChapterDefinition GetChapter(TutorialChapterId chapterId)
        {
            if (!_cacheBuilt) RebuildCache();
            return chapterId != null && _chapterRegistry.TryGet(chapterId, out var chapter) ? chapter : null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Полная валидация графа: дубли id, отсутствующие transition targets, отсутствие
        /// entry point, недостижимые шаги, отсутствие терминального шага, отсутствие чекпоинтов.
        /// Вызывается из редакторского инструмента, не из рантайма.
        /// </summary>
        public bool ValidateGraph(out List<string> errors)
        {
            errors = new List<string>();

            if (campaignId == null)
                errors.Add($"Campaign asset '{name}': campaignId is empty.");

            if (entryStepId == null)
                errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': entryStepId is empty.");

            var allSteps = new Dictionary<TutorialStepId, TutorialStepDefinition>();
            var seenChapterIds = new HashSet<TutorialChapterId>();

            foreach (var chapter in chapters)
            {
                if (chapter == null)
                {
                    errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': contains a null chapter reference.");
                    continue;
                }

                if (!seenChapterIds.Add(chapter.chapterId))
                    errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': duplicate chapterId '{chapter.chapterId?.DebugKey}'.");

                if (!chapter.Validate(out var chapterError))
                    errors.Add(chapterError);

                foreach (var step in chapter.steps)
                {
                    if (step == null) continue;
                    if (!allSteps.TryAdd(step.stepId, step))
                        errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': duplicate stepId '{step.stepId?.DebugKey}' across chapters.");
                }
            }

            if (entryStepId != null && !allSteps.ContainsKey(entryStepId))
                errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': entryStepId '{entryStepId.DebugKey}' does not match any step.");

            foreach (var step in allSteps.Values)
            {
                foreach (var t in step.transitions)
                {
                    if (t.IsTerminal) continue;
                    if (!allSteps.ContainsKey(t.nextStepId))
                        errors.Add($"Step '{step.stepId?.DebugKey}': transition points to missing stepId '{t.nextStepId?.DebugKey}'.");
                }
            }

            if (entryStepId != null && allSteps.ContainsKey(entryStepId))
            {
                var reachable = new HashSet<TutorialStepId>();
                var queue = new Queue<TutorialStepId>();
                queue.Enqueue(entryStepId);
                reachable.Add(entryStepId);

                while (queue.Count > 0)
                {
                    var current = allSteps[queue.Dequeue()];
                    foreach (var t in current.transitions)
                    {
                        if (t.IsTerminal || !allSteps.ContainsKey(t.nextStepId)) continue;
                        if (reachable.Add(t.nextStepId))
                            queue.Enqueue(t.nextStepId);
                    }
                }

                foreach (var stepId in allSteps.Keys)
                {
                    if (!reachable.Contains(stepId))
                        errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': step '{stepId?.DebugKey}' is unreachable from entry point.");
                }
            }

            bool hasTerminal = false;
            bool hasCheckpoint = false;
            foreach (var step in allSteps.Values)
            {
                if (step.transitions.Count == 0 ||
                    (step.transitions.Count > 0 && step.transitions[^1].IsTerminal))
                    hasTerminal = true;

                if (step.isCheckpoint)
                    hasCheckpoint = true;
            }

            if (!hasTerminal)
                errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': graph has no terminal step.");

            if (!hasCheckpoint)
                errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': no step has isCheckpoint = true — " +
                           "mobile interruption before any checkpoint would restart the whole campaign from entry point.");

            // Fix: цикл в графе заставляет TutorialService.ActivateStep's `while (stepId != null)`
            // крутиться бесконечно, если каждый шаг в цикле мгновенно завершается (все объективы
            // уже удовлетворены). Граф тутора должен быть DAG — проверяем это здесь, а не рантайм
            // счётчиком итераций.
            errors.AddRange(DetectCycles(allSteps));

            return errors.Count == 0;
        }

        /// <summary>DFS с раскраской (white/gray/black). Рёбра — ВСЕ non-terminal transitions
        /// шага, независимо от условия: условия — рантайм-данные, статическая валидация не может
        /// предполагать, какое из них истинно, поэтому структурный цикл невалиден даже если
        /// часть условий теоретически никогда не позволит его пройти.</summary>
        private List<string> DetectCycles(Dictionary<TutorialStepId, TutorialStepDefinition> allSteps)
        {
            var errors = new List<string>();
            var state = new Dictionary<TutorialStepId, int>(); // 0=не посещён(нет записи), 1=gray, 2=black
            var pathStack = new List<TutorialStepId>();

            foreach (var stepId in allSteps.Keys)
            {
                if (!state.ContainsKey(stepId))
                    Visit(stepId);
            }

            return errors;

            void Visit(TutorialStepId stepId)
            {
                state[stepId] = 1;
                pathStack.Add(stepId);

                foreach (var t in allSteps[stepId].transitions)
                {
                    if (t.IsTerminal || !allSteps.ContainsKey(t.nextStepId)) continue;

                    if (!state.TryGetValue(t.nextStepId, out var s))
                    {
                        Visit(t.nextStepId);
                    }
                    else if (s == 1)
                    {
                        var cycleStart = pathStack.IndexOf(t.nextStepId);
                        var cyclePath = string.Join(" -> ",
                            pathStack.GetRange(cycleStart, pathStack.Count - cycleStart).Select(id => id?.DebugKey));
                        errors.Add($"Campaign '{campaignId?.DebugKey ?? "?"}': cycle detected in graph: {cyclePath} -> {t.nextStepId?.DebugKey}");
                    }
                }

                pathStack.RemoveAt(pathStack.Count - 1);
                state[stepId] = 2;
            }
        }
#endif
    }
}
