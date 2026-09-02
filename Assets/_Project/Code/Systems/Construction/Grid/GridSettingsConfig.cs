using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// ScriptableObject конфигурации сетки строительства.
    ///
    /// Хранит все параметры grid:
    /// - размеры
    /// - смещение
    /// - размер ячейки
    /// - цвет отображения сетки (для debug/ghost)
    /// - максимальное количество объектов на сетке (optional)
    /// </summary>
    [CreateAssetMenu(
        fileName = "GridSettingsConfig",
        menuName = "Game Configs/Construction/Grid Settings Config")]
    public class GridSettingsConfig : ScriptableObject
    {
        [Header("Grid Size & Offset")] [Tooltip("Количество ячеек по X и Y")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);

        [Tooltip("Смещение сетки относительно мирового нуля")]
        [SerializeField] private Vector2Int gridOffset = Vector2Int.zero;

        [Header("Cell Settings")] [Tooltip("Размер ячейки в мировых единицах")]
        [SerializeField] private float cellSize = 1f;

        [Header("Debug Settings")] [Tooltip("Цвет отображения свободной ячейки")]
        [SerializeField] private Color freeCellColor = new Color(0, 1, 0, 0.3f);

        [Tooltip("Цвет отображения занятой ячейки")]
        [SerializeField] private Color occupiedCellColor = new Color(1, 0, 0, 0.3f);

        [Header("Optional Limits")] [Tooltip("Максимальное количество объектов на сетке (0 = не ограничено)")]
        [SerializeField] private int maxObjects = 0;


        
        
        
        public Vector2Int GridSize => gridSize;

        public Vector2Int GridOffset => gridOffset;

        public float CellSize => cellSize;

        public Color FreeCellColor => freeCellColor;

        public Color OccupiedCellColor => occupiedCellColor;

        public int MaxObjects => maxObjects;


        // =====================================================
        // Вспомогательные свойства
        // =====================================================

        /// <summary>Проверка валидного размера сетки</summary>
        public bool IsValidSize => gridSize.x > 0 && gridSize.y > 0 && cellSize > 0;

        /// <summary>Общее количество ячеек сетки</summary>
        public int TotalCells => gridSize.x * gridSize.y;

    }
}