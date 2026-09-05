using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Core.State;
using R3;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Рантайм-представление активной кампании. Персистентная часть — ReactiveProperty
    /// на struct CGameStateTutorial (проектный паттерн для плоских глобальных состояний,
    /// см. GameStateProxy.IAP/ADState/Review). Мутации — через StateWriter.Write.
    ///
    /// CGameStateTutorial.currentStepId/checkpointStepId/completedStepIds остаются string —
    /// ScriptableObject-ссылку (TutorialStepId) нельзя напрямую сериализовать в сейв.
    /// Persist всегда пишет stepId.Guid; чтение обратно в TutorialStepDefinition — через
    /// TutorialDefinition.GetStepByGuid (см. её докстринг).
    ///
    /// НЕТ отдельного HashSet-кэша completedStepIds (убран в P0 corrective pass) —
    /// единственный источник истины это CGameStateTutorial.Value.completedStepIds,
    /// читается напрямую на каждый запрос. Список шагов тутора мал (десятки, не тысячи),
    /// поэтому List.Contains() не является проблемой производительности, а отсутствие
    /// кэша исключает саму возможность рассинхронизации.
    /// </summary>
    public sealed class TutorialRuntime
    {
        public readonly TutorialDefinition Definition;
        private readonly ReactiveProperty<CGameStateTutorial> _state;

        public TutorialStepRuntimeState CurrentStep { get; private set; }

        public string CampaignId => _state.Value.campaignId;
        public string CurrentStepId => _state.Value.currentStepId;
        public string CheckpointStepId => _state.Value.checkpointStepId;
        public bool IsActive => !string.IsNullOrEmpty(CampaignId) && !_state.Value.completed;
        public bool IsCompleted => _state.Value.completed;

        public TutorialRuntime(TutorialDefinition definition, ReactiveProperty<CGameStateTutorial> state)
        {
            Definition = definition;
            _state = state;
        }

        public TutorialStepDefinition GetCurrentStepDefinition()
            => string.IsNullOrEmpty(CurrentStepId) ? null : Definition.GetStepByGuid(CurrentStepId);

        public bool IsStepCompleted(TutorialStepId stepId)
            => stepId != null && _state.Value.completedStepIds != null && _state.Value.completedStepIds.Contains(stepId.Guid);

        /// <summary>Только обновляет currentStepId в памяти. Не персистит — TutorialService
        /// контролирует, когда именно вызывается SaveGameState().</summary>
        public void SetActiveStep(TutorialStepRuntimeState step)
        {
            CurrentStep?.Stop();
            CurrentStep = step;
            var guid = step?.Definition.stepId?.Guid;
            StateWriter.Write(_state, (ref CGameStateTutorial t) => t.currentStepId = guid);
        }

        /// <summary>Помечает шаг ГЕНУИННО завершённым (не Skipped — см. TutorialService,
        /// Skip никогда не вызывает этот метод).</summary>
        public void MarkStepCompleted(TutorialStepId stepId)
        {
            if (stepId == null) return;
            var guid = stepId.Guid;
            var current = _state.Value.completedStepIds ?? new System.Collections.Generic.List<string>();
            if (!current.Contains(guid))
            {
                var updated = current.ToList();
                updated.Add(guid);
                StateWriter.Write(_state, (ref CGameStateTutorial t) => t.completedStepIds = updated);
            }
        }

        public void MarkCampaignCompleted()
        {
            CurrentStep?.Stop();
            CurrentStep = null;
            StateWriter.Write(_state, (ref CGameStateTutorial t) =>
            {
                t.completed = true;
                t.currentStepId = null;
            });
        }

        public TutorialProgress ToProgress(TutorialChapterId currentChapterId)
        {
            var snapshot = _state.Value;

            // Fix: раньше сюда передавался snapshot.completedStepIds напрямую — тот же
            // List<string>, на который ссылается CGameStateTutorial. Внешний код, получивший
            // TutorialProgress через GetProgress(), мог мутировать persistent-состояние тутора
            // через этот список. Защитная копия делает TutorialProgress настоящим снэпшотом.
            var completedCopy = snapshot.completedStepIds != null
                ? new List<string>(snapshot.completedStepIds)
                : new List<string>();

            return new TutorialProgress(
                snapshot.campaignId, currentChapterId?.DebugKey, snapshot.currentStepId, snapshot.checkpointStepId,
                snapshot.completed, IsActive, completedCopy);
        }
    }
}
