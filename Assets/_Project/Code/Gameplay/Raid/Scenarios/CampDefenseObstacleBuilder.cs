using System;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Grid;
using Pathfinding;

namespace Galactic1.Code.Systems.CampDefense.Navigation
{
    /// <summary>
    /// Временная блокировка навигационной сетки (A* Grid Graph) во время Camp Defense.
    ///
    /// Отвечает ТОЛЬКО за:
    /// - обход построенных зданий лагеря
    /// - получение их NavigationBounds
    /// - применение GraphUpdateObject
    /// - восстановление графа после завершения режима
    ///
    /// Ничего не знает о волнах, орде, UI, GameLoop, победе/поражении —
    /// используется исключительно как утилита сценарием Camp Defense.
    /// </summary>
    public sealed class CampDefenseObstacleBuilder : IDisposable
    {
        private const float BoundsExpand = 0.25f;

        private readonly BaseFacilityRepository _facilityRepository;
        private bool _built;

        public CampDefenseObstacleBuilder(BaseFacilityRepository facilityRepository)
        {
            _facilityRepository = facilityRepository;
        }

        /// <summary>
        /// Блокирует граф под всеми зданиями лагеря.
        /// Вызывается ровно 1 раз при запуске Camp Defense.
        /// </summary>
        public void Build()
        {
            if (_built)
                return;

            foreach (var instance in _facilityRepository.All.Values)
                NavigationBlocker.Block(instance.NavigationBounds);

            AstarPath.active.FlushGraphUpdates();
            _built = true;
        }

        /// <summary>
        /// Полностью восстанавливает граф в исходное состояние.
        /// Вызывается ровно 1 раз при завершении Camp Defense.
        /// Camp после защиты полностью перезагружается, поэтому достаточно Scan()
        /// без ручного отката отдельных GraphUpdateObject.
        /// </summary>
        public void Dispose()
        {
            if (!_built)
                return;

            _built = false;
        }
    }
}