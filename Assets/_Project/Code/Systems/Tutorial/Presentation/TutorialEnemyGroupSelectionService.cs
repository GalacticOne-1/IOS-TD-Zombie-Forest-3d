using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Единая точка резолва "N ближайших живых врагов к origin-таргету". Selection фиксируется
    /// один раз в Begin() и дальше только уменьшается через Remove() — никогда не пересчитывается
    /// заново (см. док по Chapter 2 enemy search: "начало: A B C selected, A killed → B C",
    /// а не пересчёт "ближайших трёх" после каждой смерти).
    ///
    /// Владелец жизненного цикла selection — EnemyGroupKilledObjective: вызывает Begin() из
    /// своего EvaluateCurrentState() (единственный доступный хук перед стартом подписки на
    /// событие, т.к. Start()/Stop() базового класса не virtual). Presentation
    /// (TutorialEnemySearchResolver) только читает уже существующую selection, никогда её
    /// не создаёт.
    /// </summary>
    public sealed class TutorialEnemyGroupSelectionService : IGameService
    {
        private sealed class Selection
        {
            public readonly List<string> AliveIds = new();
            public int ResolvedTotal;
            public bool Resolved;
        }

        private readonly TutorialTargetRegistry _targetRegistry;
        private readonly EnemyRepository _enemyRepository;

        private readonly Dictionary<TutorialTargetId, Selection> _selections = new();
        private readonly Dictionary<TutorialTargetId, Action<ITutorialTarget>> _pendingOriginHandlers = new();

        public event Action<TutorialTargetId> OnSelectionChanged;

        public TutorialEnemyGroupSelectionService(
            TutorialTargetRegistry targetRegistry,
            EnemyRepository enemyRepository)
        {
            _targetRegistry = targetRegistry;
            _enemyRepository = enemyRepository;
        }

        /// <summary>Фиксирует selection для originTargetId, перезаписывая предыдущую (перезапуск
        /// шага естественным образом сбрасывает набор). Если origin ещё не на сцене — ждёт
        /// регистрации (тот же паттерн, что TutorialPresentationService.ResolveTarget).</summary>
        public void Begin(TutorialTargetId originTargetId, int count)
        {
            if (originTargetId == null) return;

            if (_pendingOriginHandlers.TryGetValue(originTargetId, out var stale))
            {
                _targetRegistry.OnTargetRegistered -= stale;
                _pendingOriginHandlers.Remove(originTargetId);
            }

            var selection = new Selection();
            _selections[originTargetId] = selection;

            if (_targetRegistry.TryGetTarget(originTargetId, out var origin))
            {
                Resolve(originTargetId, selection, origin, count);
                return;
            }

            void Handler(ITutorialTarget registered)
            {
                if (registered.TargetId != originTargetId) return;
                _targetRegistry.OnTargetRegistered -= Handler;
                _pendingOriginHandlers.Remove(originTargetId);

                // Только если это всё ещё актуальная (не перезаписанная новым Begin) selection.
                if (_selections.TryGetValue(originTargetId, out var current) && current == selection)
                    Resolve(originTargetId, selection, registered, count);
            }

            _pendingOriginHandlers[originTargetId] = Handler;
            _targetRegistry.OnTargetRegistered += Handler;
        }

        /// <summary>Вызывается объективом при совпадении EnemyKilledEvent с его группой.
        /// Возвращает true, если id действительно входил в selection.</summary>
        public bool Remove(TutorialTargetId originTargetId, string enemyId)
        {
            if (originTargetId == null || enemyId == null) return false;
            if (!_selections.TryGetValue(originTargetId, out var selection)) return false;
            if (!selection.AliveIds.Remove(enemyId)) return false;

            OnSelectionChanged?.Invoke(originTargetId);
            return true;
        }

        public int AliveCount(TutorialTargetId originTargetId)
            => _selections.TryGetValue(originTargetId, out var s) ? s.AliveIds.Count : 0;

        /// <summary>Фактически найденное количество врагов (может быть меньше запрошенного
        /// count, если рядом с origin их физически меньше). 0, пока не резолвлено.</summary>
        public int ResolvedTotal(TutorialTargetId originTargetId)
            => _selections.TryGetValue(originTargetId, out var s) ? s.ResolvedTotal : 0;

        /// <summary>true только если резолв уже произошёл и вокруг origin не нашлось ни одного
        /// врага — вырожденный случай, чтобы объектив не завис навсегда.</summary>
        public bool IsResolvedEmpty(TutorialTargetId originTargetId)
            => _selections.TryGetValue(originTargetId, out var s) && s.Resolved && s.ResolvedTotal == 0;

        /// <summary>Ближайший ещё живой враг группы — для highlight/arrow. Пересчитывает
        /// дистанцию каждый вызов (враги двигаются), но НЕ меняет состав selection.</summary>
        public bool TryGetNearestAlive(TutorialTargetId originTargetId, out EnemyInstance enemy)
        {
            enemy = null;

            if (!_selections.TryGetValue(originTargetId, out var selection) || selection.AliveIds.Count == 0)
                return false;

            if (!_targetRegistry.TryGetTarget(originTargetId, out var origin) || origin.WorldAnchor == null)
                return false;

            Vector3 originPos = origin.WorldAnchor.position;
            float bestDist = float.MaxValue;

            foreach (var id in selection.AliveIds)
            {
                var (done, instance) = _enemyRepository.TryGet(id);
                if (!done) continue;

                float dist = (instance.transform.position - originPos).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    enemy = instance;
                }
            }

            return enemy != null;
        }

        private void Resolve(TutorialTargetId originTargetId, Selection selection, ITutorialTarget origin, int count)
        {
            if (origin.WorldAnchor == null)
            {
                Debug.LogError($"[TutorialEnemyGroupSelectionService] Origin '{originTargetId.DebugKey}' has no " +
                                "WorldAnchor — enemy search requires a world target, not a UI target.");
                selection.Resolved = true;
                return;
            }

            Vector3 originPos = origin.WorldAnchor.position;

            // NOTE: предполагается, что EnemyRepository.ActiveEnemies уже содержит только живых
            // врагов (юнит удаляется из репозитория при смерти) — своей проверки IsDead здесь
            // нет, т.к. у меня нет доступа к ISceneEnemy/IUnitRuntimeBase, чтобы её корректно
            // написать. Проверьте это допущение перед мержем.
            var nearest = _enemyRepository.ActiveEnemies
                .Where(e => e != null)
                .OrderBy(e => (e.transform.position - originPos).sqrMagnitude)
                .Take(count)
                .Select(e => e.EnemyAdapter.Runtime.Id)
                .ToList();

            selection.AliveIds.AddRange(nearest);
            selection.ResolvedTotal = nearest.Count;
            selection.Resolved = true;

            OnSelectionChanged?.Invoke(originTargetId);
        }
    }
}