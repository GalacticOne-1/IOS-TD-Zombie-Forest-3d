using System.Collections.Generic;
using Galactic1.Code.Gameplay.Grid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Отвечает за визуализацию сетки в режиме строительства.
    /// Показывает клетки вокруг ghost-объекта.
    /// Меняет цвет клеток в зависимости от валидности размещения (зеленый/красный).
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        [Header("Grid Settings")] 
        public Transform root;
        public GameObject cellPrefab;     // Префаб одной клетки
        public Material validMaterial;    // Зеленая
        public Material invalidMaterial;  // Красная

        private GameObject[,] cellsGrid;
        private int width;
        private int height;

        private Material defaultMaterial;
        
        private List<Vector2Int> _lastCells = new();
        private List<Vector2Int> _originCells = new();
        

        /// <summary>
        /// Создает визуальную сетку заданного размера
        /// </summary>
        public void CreateGrid(GridSettingsConfig config)
        {
            width = config.GridSize.x;
            height = config.GridSize.y;
            var origin = config.GridOffset;
            var cellSize = config.CellSize;
            cellsGrid = new GameObject[width, height];

            defaultMaterial = cellPrefab.GetComponent<Renderer>().sharedMaterial;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject cell = Instantiate(cellPrefab, root);
                    cell.transform.localScale = Vector3.one * cellSize;
                    cell.transform.position =
                        new Vector3(
                            origin.x + x * cellSize + cellSize * 0.5f,
                            0.3f,
                            origin.y + y * cellSize + cellSize * 0.5f
                        );
                    cellsGrid[x, y] = cell;
                }
            }
        }

        /// <summary>
        /// Обновляет цвет клеток на основе массива валидности
        /// </summary>
        public void HighlightCells(
            List<Vector2Int> cells,
            bool valid)
        {
            ClearLast();

            var mat = valid ? validMaterial : invalidMaterial;

            foreach (var c in cells)
            {
                if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height)
                    continue;

                var renderer = cellsGrid[c.x, c.y].GetComponent<Renderer>();
                renderer.sharedMaterial = mat;

                _lastCells.Add(c);
            }
        }
        
        public void HighlightOrigin(List<Vector2Int> cells)
        {
            ClearOrigin();

            foreach (var c in cells)
            {
                if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height)
                    continue;

                var renderer = cellsGrid[c.x, c.y].GetComponent<Renderer>();
                renderer.sharedMaterial = validMaterial;

                _originCells.Add(c);
            }
        }
        
        /// <summary>
        /// Красит клетки препятствий (obstacle) красным при включении режима строительства.
        /// </summary>
        public void RenderObstacles(GridService grid)
        {
            var occupied = grid.GetOccupiedCells();

            foreach (var cell in occupied)
            {
                if (!grid.IsObstacle(cell))
                    continue;

                if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
                    continue;

                var renderer = cellsGrid[cell.x, cell.y].GetComponent<Renderer>();
                renderer.sharedMaterial = invalidMaterial;
            }
        }
        
        
        void ClearLast()
        {
            foreach (var c in _lastCells)
            {
                var renderer = cellsGrid[c.x, c.y].GetComponent<Renderer>();
                renderer.sharedMaterial = defaultMaterial;
            }

            _lastCells.Clear();
        }

        void ClearOrigin()
        {
            foreach (var c in _originCells)
            {
                var renderer = cellsGrid[c.x, c.y].GetComponent<Renderer>();
                renderer.sharedMaterial = defaultMaterial;
            }

            _originCells.Clear();
        }
        
        
        public void Clear()
        {
            ClearLast();
        }
        public void Reset(GridService grid)
        {
            Clear();
            ClearOrigin();
            RenderObstacles(grid);
        }
        
        /// <summary>
        /// Показывает сетку
        /// </summary>
        public void ShowGrid()
        {
            root.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Скрывает сетку
        /// </summary>
        public void HideGrid()
        {
            root.gameObject.SetActive(false);
        }

    }
}