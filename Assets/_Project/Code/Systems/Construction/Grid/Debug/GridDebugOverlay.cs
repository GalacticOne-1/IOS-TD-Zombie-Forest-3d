using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid.Debug
{
    /// <summary>
    /// Debug overlay строительной сетки.
    /// Показывает индексы клеток, границы и статические заблокированные зоны.
    /// Используется только в dev/debug режиме.
    /// </summary>
    public class GridDebugOverlay : MonoBehaviour
    {
        public GridSettingsConfig config;
        public GridBlockedAreasConfig blockedAreasConfig;

        public bool showIndices = true;
        public bool showCenters = true;
        public bool showBlockedAreas = true;

        [SerializeField] private Color blockedAreaColor = new Color(1f, 0f, 0f, 0.35f);

        void OnDrawGizmos()
        {
            if (config == null)
                return;

            float cellSize = config.CellSize;
            var origin = config.GridOffset;
            var size = config.GridSize;

            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                Vector3 center = new Vector3(
                    origin.x + x * cellSize + cellSize * 0.5f,
                    0,
                    origin.y + y * cellSize + cellSize * 0.5f
                );

                if (showCenters)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(center, cellSize * 0.05f);
                }

#if UNITY_EDITOR
                if (showIndices)
                {
                    UnityEditor.Handles.Label(
                        center + Vector3.up * 0.1f,
                        $"{x},{y}"
                    );
                }
#endif
            }

            DrawBlockedAreas(cellSize, origin);
        }

        void DrawBlockedAreas(float cellSize, Vector2Int origin)
        {
            if (!showBlockedAreas || blockedAreasConfig == null)
                return;

            Gizmos.color = blockedAreaColor;

            foreach (var area in blockedAreasConfig.BlockedAreas)
            {
                for (int x = 0; x < area.Width; x++)
                for (int y = 0; y < area.Height; y++)
                {
                    Vector3 center = new Vector3(
                        origin.x + (area.Origin.x + x) * cellSize + cellSize * 0.5f,
                        0,
                        origin.y + (area.Origin.y + y) * cellSize + cellSize * 0.5f
                    );

                    Gizmos.DrawCube(
                        center,
                        new Vector3(cellSize, 0.02f, cellSize));
                }
            }
        }
    }
}