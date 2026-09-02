using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Сервис статических заблокированных зон сетки.
    ///
    /// Роль:
    /// - разворачивает прямоугольники из GridBlockedAreasConfig в клетки с тегами
    /// - хранит их в Dictionary для быстрой проверки
    /// - read-only в билде после инициализации
    ///
    /// Не связан с GridService: статические зоны и динамическая occupancy
    /// — раздельные системы.
    /// </summary>
    public class GridBlockedAreaService
    {
        private readonly Dictionary<Vector2Int, GridZoneTag> _blockedCells = new();
        private readonly GridBlockedAreasConfig _config;

        public IReadOnlyDictionary<Vector2Int, GridZoneTag> BlockedCells => _blockedCells;

        public GridBlockedAreaService(GridBlockedAreasConfig config)
        {
            _config = config;
            Rebuild();

#if UNITY_EDITOR
            if (_config != null)
                _config.Changed += Rebuild;
#endif
        }

        /// <summary>
        /// Пересобирает Dictionary из текущего состояния конфига.
        /// В билде вызывается только один раз, при инициализации.
        /// В эдиторе может пересобираться на правку ассета (dev-удобство).
        /// </summary>
        public void Rebuild()
        {
            _blockedCells.Clear();

            if (_config == null) return;

            foreach (var area in _config.BlockedAreas)
            {
                for (int x = 0; x < area.Width; x++)
                for (int y = 0; y < area.Height; y++)
                {
                    var cell = new Vector2Int(
                        area.Origin.x + x,
                        area.Origin.y + y);

                    _blockedCells[cell] = area.Tag;
                }
            }
        }

        public bool IsBlocked(Vector2Int cell)
        {
            return _blockedCells.ContainsKey(cell);
        }

        public bool IsBlocked(IEnumerable<Vector2Int> cells)
        {
            foreach (var c in cells)
                if (_blockedCells.ContainsKey(c))
                    return true;

            return false;
        }

        /// <summary>
        /// Возвращает тег зоны, если клетка заблокирована.
        /// </summary>
        public bool TryGetTag(Vector2Int cell, out GridZoneTag tag)
        {
            return _blockedCells.TryGetValue(cell, out tag);
        }
    }
}