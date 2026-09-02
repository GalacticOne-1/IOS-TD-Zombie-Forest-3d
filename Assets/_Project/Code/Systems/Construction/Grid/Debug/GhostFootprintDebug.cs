using UnityEngine;
using Galactic1.Code.Gameplay.Construction;

namespace Galactic1.Code.Gameplay.Grid.Debug
{

    public class GhostFootprintDebug : MonoBehaviour
    {
        public ConstructionPlacementController placement;
        public GridSettingsConfig config;

        void OnDrawGizmos()
        {
            if (placement == null || placement.CurrentGhost == null)
                return;

            var footprint = placement.CurrentGhost.FootprintRuntime;
            var origin = config.GridOffset;
            float cellSize = config.CellSize;

            foreach (var cell in footprint.Cells)
            {
                Vector3 center = new Vector3(
                    origin.x + cell.x * cellSize + cellSize * 0.5f,
                    0.05f,
                    origin.y + cell.y * cellSize + cellSize * 0.5f
                );

                Gizmos.color = Color.green;

                Gizmos.DrawCube(
                    center,
                    new Vector3(cellSize, 0.02f, cellSize)
                );
            }
        }
    }
}