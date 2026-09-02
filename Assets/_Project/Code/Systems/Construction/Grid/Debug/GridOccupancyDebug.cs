using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid.Debug
{
    /// <summary>
    /// Показывает занятые клетки сетки.
    /// </summary>
    public class GridOccupancyDebug : MonoBehaviour
    {
        public GridService grid;
        public GridSettingsConfig config;

        // void OnDrawGizmos()
        // {
        //     if (grid == null || config == null)
        //         return;
        //
        //     float cellSize = config.CellSize;
        //     var origin = config.GridOffset;
        //
        //     foreach (var cell in grid.GetOccupiedCells())
        //     {
        //         Vector3 center = new Vector3(
        //             origin.x + cell.x * cellSize + cellSize * 0.5f,
        //             0.02f,
        //             origin.y + cell.y * cellSize + cellSize * 0.5f
        //         );
        //
        //         Gizmos.color = Color.red;
        //
        //         Gizmos.DrawCube(
        //             center,
        //             new Vector3(cellSize, 0.02f, cellSize)
        //         );
        //     }
        // }
    }
}