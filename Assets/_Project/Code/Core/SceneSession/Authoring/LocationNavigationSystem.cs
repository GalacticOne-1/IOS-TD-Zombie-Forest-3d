using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.Utils;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Navigation
{
    /// <summary>
    /// Сторона локации, которая считается "передней" — на ней паддинг
    /// (обрезка/расширение сетки) не применяется.
    /// </summary>
    public enum LocationFrontSide
    {
        None, // паддинг одинаковый со всех сторон (старое поведение)
        PositiveX,
        NegativeX,
        PositiveZ,
        NegativeZ
    }

    public class LocationNavigationSystem
    {
        private const int PaddingCellsPerSide = 20;

        public GridGraph Configure(GridGraphConfigurationDTO dto)
        {
            var settings = dto.Settings;
            var locationSize = dto.LocationSize;
            
            // !! размер сетки навигации всегда должен быть четным что бы расставленные коллайдеры не плыли !!
            locationSize.x = EvenSnapUtility.ToNearestEven(locationSize.x);
            locationSize.y = EvenSnapUtility.ToNearestEven(locationSize.y);

            if (AstarPath.active == null)
            {
                Debug.LogError("[AstarGridGraphConfigurator] AstarPath.active == null. " +
                               "Убедитесь, что на сцене есть объект с компонентом AstarPath.");
                return null;
            }

            var gridGraph = AstarPath.active.data.gridGraph;
            if (gridGraph == null)
            {
                gridGraph = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
            }

            if (gridGraph == null)
            {
                Debug.LogError("[AstarGridGraphConfigurator] Не удалось создать/получить GridGraph.");
                return null;
            }

            if (settings.NodeSize <= 0f)
            {
                Debug.LogError("[AstarGridGraphConfigurator] NodeSize должен быть больше нуля.");
                return null;
            }

            int baseWidthNodes = Mathf.Max(1, Mathf.RoundToInt(locationSize.x / settings.NodeSize));
            int baseDepthNodes = Mathf.Max(1, Mathf.RoundToInt(locationSize.y / settings.NodeSize));

            // По умолчанию — без паддинга нигде
            int padXNeg = 0;
            int padXPos = 0;
            int padZNeg = 0;
            int padZPos = 0;

            if (settings.FrontSide == LocationFrontSide.None)
            {
                // старое поведение — паддинг одинаковый со всех сторон
                padXNeg = PaddingCellsPerSide;
                padXPos = PaddingCellsPerSide;
                padZNeg = PaddingCellsPerSide;
                padZPos = PaddingCellsPerSide;
            }
            else
            {
                // паддинг только на выбранной стороне, остальные три — срезаны вровень с локацией
                switch (settings.FrontSide)
                {
                    case LocationFrontSide.PositiveX:
                        padXPos = PaddingCellsPerSide;
                        break;
                    case LocationFrontSide.NegativeX:
                        padXNeg = PaddingCellsPerSide;
                        break;
                    case LocationFrontSide.PositiveZ:
                        padZPos = PaddingCellsPerSide;
                        break;
                    case LocationFrontSide.NegativeZ:
                        padZNeg = PaddingCellsPerSide;
                        break;
                }
            }

            int widthNodes = baseWidthNodes + padXNeg + padXPos;
            int depthNodes = baseDepthNodes + padZNeg + padZPos;

            gridGraph.SetDimensions(widthNodes, depthNodes, settings.NodeSize);

            // Сдвиг центра: сторона С паддингом "тянет" центр в свою сторону,
            // стороны без паддинга остаются на месте.
            float centerOffsetX = (padXPos - padXNeg) * 0.5f * settings.NodeSize;
            float centerOffsetZ = (padZPos - padZNeg) * 0.5f * settings.NodeSize;

            var center = gridGraph.center;
            center.x += centerOffsetX;
            center.z += centerOffsetZ;
            gridGraph.center = center;

            gridGraph.collision.diameter = settings.CollisionDiameter;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(AstarPath.active);
#endif

            Debug.Log($"[AstarGridGraphConfigurator] GridGraph настроен: " +
                      $"center={gridGraph.center}, dimensions={widthNodes}x{depthNodes} " +
                      $"(locationSize={locationSize}, frontSide={settings.FrontSide}, " +
                      $"padding X:[{padXNeg},{padXPos}] Z:[{padZNeg},{padZPos}]), " +
                      $"nodeSize={settings.NodeSize}, collisionDiameter={settings.CollisionDiameter}.");

            TriggerScanIfNeeded(settings, gridGraph);

            return gridGraph;
        }

        private void TriggerScanIfNeeded(
            LocationGeometryDefinition.NavigationSettings settings,
            GridGraph gridGraph)
        {
            if (!settings.ScanOnBuild)
                return;

            // AstarPath.active.Scan(gridGraph);

            Debug.Log("[AstarGridGraphConfigurator] ScanOnBuild = true, но Scan ещё не реализован " +
                      "(зарезервировано под будущую задачу).");
        }
    }
}