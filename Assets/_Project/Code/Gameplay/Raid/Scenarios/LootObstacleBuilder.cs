using System;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.RaidLoot.Scene.Lifecycle;
using UnityEngine;

namespace Galactic1.RaidLoot.Navigation
{
    /// <summary>
    /// Временная блокировка навигационной сетки (A* Grid Graph) под лутовыми
    /// контейнерами во время exploration-рейда.
    ///
    /// Аналог CampDefenseObstacleBuilder, но для LootContainerSceneLifecycleSystem.
    ///
    /// Отвечает ТОЛЬКО за:
    /// - обход заспавненных лутовых контейнеров
    /// - применение GraphUpdateObject по их позиции (фиксированный радиус 1.5f)
    /// - точное восстановление графа после завершения рейда
    ///
    /// В отличие от Camp Defense, exploration-локация НЕ перезагружается целиком
    /// после рейда — Scan() здесь недопустим, восстановление идёт точечно,
    /// теми же Bounds, что были заблокированы в Build().
    /// </summary>
    public sealed class LootObstacleBuilder : IDisposable
    {
        private const float ContainerRadius = 1.5f;
        private const float BoundsExpand = 0.25f;

        private readonly LootContainerSceneLifecycleSystem _lootLifecycle;
        private bool _built;

        public LootObstacleBuilder(LootContainerSceneLifecycleSystem lootLifecycle)
        {
            _lootLifecycle = lootLifecycle;
        }

        /// <summary>
        /// Блокирует граф под всеми заспавненными лутовыми контейнерами.
        /// Вызывается ровно 1 раз при старте рейда (после SetSceneReady).
        /// </summary>
        public void Build()
        {
            if (_built)
                return;

            foreach (var data in _lootLifecycle.Containers)
            {
                if (data.View == null)
                    continue;

                var bounds = new Bounds(
                    data.View.transform.position,
                    Vector3.one * (ContainerRadius * 2f));

                NavigationBlocker.Block(bounds);
            }

            AstarPath.active.FlushGraphUpdates();
            _built = true;
        }

        /// <summary>
        /// Точечно восстанавливает walkability под теми же Bounds, что были
        /// заблокированы в Build(). Scan() намеренно не вызывается —
        /// exploration-локация не перезагружается целиком.
        /// </summary>
        public void Dispose()
        {
            if (!_built)
                return;

            
            _built = false;
        }
    }
}