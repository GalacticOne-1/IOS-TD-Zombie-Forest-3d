using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Сервис координат строительной сетки.
    ///
    /// Роль:
    /// - конвертация world ↔ cell
    /// - snap к сетке
    /// - получение клеток footprint с anchor bottom-left
    /// - проверка границ сетки
    /// </summary>
    public class GridCoordinateService
    {
        private readonly float _cellSize;
        private readonly Vector2Int _origin;
        private readonly Vector2Int _gridSize;
        public bool DebugLogConversions;

        public float CellSize => _cellSize;

        public GridCoordinateService(
            float cellSize,
            Vector2Int origin,
            Vector2Int gridSize)
        {
            _cellSize = cellSize;
            _origin = origin;
            _gridSize = gridSize;
        }

        // =====================================================
        // WORLD → CELL
        // =====================================================
        public Vector2Int WorldToCell(Vector3 world)
        {
            int x = Mathf.FloorToInt((world.x - _origin.x) / _cellSize);
            int y = Mathf.FloorToInt((world.z - _origin.y) / _cellSize);

            var cell = new Vector2Int(x, y);
            if (DebugLogConversions)
                DLog.Alert($"World {world} -> Cell {cell}", EDlogColor.YELLOW);
            return cell;
        }

        // =====================================================
        // CELL → WORLD
        // =====================================================
        public Vector3 CellToWorld(Vector2Int cell)
        {
            Vector3 world = new Vector3(
                _origin.x + cell.x * _cellSize,
                0,
                _origin.y + cell.y * _cellSize
            );

            if (DebugLogConversions)
                DLog.Alert($"Cell {cell} -> World {world}", EDlogColor.YELLOW);

            return world;
        }

        // =====================================================
        // FOOTPRINT CELLS (anchor = bottom-left)
        // =====================================================
        public List<Vector2Int> GetFootprintCells(Vector2Int anchorCell, int width, int height)
        {
            List<Vector2Int> cells = new(width * height);

            // Anchor = bottom-left, поэтому идём вверх и вправо
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                cells.Add(new Vector2Int(anchorCell.x + x, anchorCell.y + y));
            }

            if (DebugLogConversions)
                DLog.Alert($"Anchor {anchorCell}, footprint size {width}x{height} -> Cells: {string.Join(", ", cells)}", EDlogColor.YELLOW);

            return cells;
        }
        
        // =====================================================
        // FOOTPRINT CENTER
        // =====================================================
        public Vector3 GetFootprintCenter(
            Vector2Int origin,
            int width,
            int height)
        {
            var world = CellToWorld(origin);

            world.x += width * _cellSize * 0.5f;
            world.z += height * _cellSize * 0.5f;

            return world;
        }

        // =====================================================
        // GRID CHECK
        // =====================================================
        public bool IsInsideGrid(Vector2Int cell)
        {
            return
                cell.x >= 0 &&
                cell.y >= 0 &&
                cell.x < _gridSize.x &&
                cell.y < _gridSize.y;
        }

        // =====================================================
        // CELL CENTER
        // =====================================================
        public Vector3 GetCellCenter(Vector2Int cell)
        {
            Vector3 world = CellToWorld(cell);
            world.x += _cellSize * 0.5f;
            world.z += _cellSize * 0.5f;
            return world;
        }

        // =====================================================
        // Snap к клетке
        // =====================================================
        public Vector3 SnapWorld(Vector3 world)
        {
            var cell = WorldToCell(world);
            return CellToWorld(cell);
        }
    }
}