using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.Gameplay.Locations.Definitions;
using Galactic1.Gameplay.Locations.Navigation;
using UnityEditor;
using UnityEngine;

namespace Galactic1.EditorTools.LocationBuilder
{
    /// <summary>
    /// Editor-инструмент сборки локации. Читает ТОЛЬКО LocationGeometryDefinition
    /// и ничего не хранит самостоятельно — по SRP: конфиг живёт в authoring-компоненте,
    /// этот класс — чистый builder (Editor-only utility).
    ///
    /// Ничего не знает о SceneDefinition, рантайме или спавн-системах.
    ///
    /// Текущие обязанности:
    ///  1) генерация пола плитками;
    ///  2) авто-расстановка 4 кардинальных SceneExitZone;
    ///  3) подготовка (не сканирование) GridGraph под Aron Granberg A*.
    ///
    /// Будущие обязанности (архитектурно предусмотрены, не реализованы):
    ///  - генерация стен;
    ///  - генерация POI root-объектов;
    ///  - генерация Spawn root-объектов.
    /// </summary>
    [CustomEditor(typeof(LocationGeometryDefinition))]
    public sealed class LocationBuilderToolEditor : Editor
    {
        private static readonly ExitId[] CardinalExits =
        {
            ExitId.ExitWest, ExitId.ExitEast, ExitId.ExitNorth, ExitId.ExitSouth
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var geometry = (LocationGeometryDefinition)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== BUILD TOOLS ===", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(geometry.TilePrefab == null))
            {
                if (GUILayout.Button("Generate Floor Tiles", GUILayout.Height(28)))
                    GenerateFloorTiles(geometry);
            }

            using (new EditorGUI.DisabledScope(geometry.ExitPrefab == null))
            {
                if (GUILayout.Button("Place Exit Zones", GUILayout.Height(28)))
                    PlaceExitZones(geometry);
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Configure Navigation (GridGraph)", GUILayout.Height(28)))
                ConfigureNavigation(geometry);

            // ---- Заготовки под будущие шаги (не реализованы) ----
            // if (GUILayout.Button("Generate Walls")) GenerateWalls(geometry);
            // if (GUILayout.Button("Create POI Root")) CreatePOIRoot(geometry);
            // if (GUILayout.Button("Create Spawn Roots")) CreateSpawnRoots(geometry);
        }

        // ===========================================================
        // FLOOR TILES
        // ===========================================================
        private void GenerateFloorTiles(LocationGeometryDefinition geometry)
        {
            if (geometry.TilePrefab == null)
            {
                Debug.LogError("[LocationBuilder] TilePrefab не назначен.");
                return;
            }

            var floorRoot = geometry.GetOrCreateRoot(geometry.FloorRootName);
            Undo.RegisterFullObjectHierarchyUndo(floorRoot.gameObject, "Generate Floor Tiles");

            // очищаем предыдущую сетку
            for (int i = floorRoot.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(floorRoot.GetChild(i).gameObject);

            var tileSize = geometry.TileSize;
            var tileCount = geometry.TileCount;
            var spacing = geometry.TileSpacing;
            var offset = geometry.GridOffset;

            if (tileSize.x <= 0f || tileSize.y <= 0f)
            {
                Debug.LogError("[LocationBuilder] TileSize должен быть больше нуля.");
                return;
            }

            if (tileCount.x <= 0 || tileCount.y <= 0)
            {
                Debug.LogError("[LocationBuilder] TileCount должен быть больше нуля по обеим осям.");
                return;
            }

            float stepX = tileSize.x + spacing.x;
            float stepZ = tileSize.y + spacing.y;

            float totalWidth = stepX * tileCount.x;
            float totalDepth = stepZ * tileCount.y;

            float startX = -totalWidth / 2f + tileSize.x / 2f + offset.x;
            float startZ = -totalDepth / 2f + tileSize.y / 2f + offset.y;

            float prefabScaleY = geometry.TilePrefab.transform.localScale.y;

            for (int ix = 0; ix < tileCount.x; ix++)
            {
                for (int iz = 0; iz < tileCount.y; iz++)
                {
                    var pos = new Vector3(
                        startX + ix * stepX,
                        geometry.FloorHeight,
                        startZ + iz * stepZ);

                    var instance = InstantiatePrefab(geometry.TilePrefab, floorRoot);
                    instance.transform.localPosition = pos;
                    instance.name = $"Tile_{ix}_{iz}";
                    instance.transform.localScale = new Vector3(tileSize.x, prefabScaleY, tileSize.y);

                    Undo.RegisterCreatedObjectUndo(instance, "Generate Floor Tiles");
                }
            }

            Debug.Log($"[LocationBuilder] Сгенерировано {tileCount.x * tileCount.y} плиток " +
                      $"({tileCount.x}x{tileCount.y}) в '{floorRoot.name}', " +
                      $"tileSize = {tileSize}, spacing = {spacing}, offset = {offset}, " +
                      $"итоговый размер сетки = {totalWidth}x{totalDepth}.");
        }

        // ===========================================================
        // EXIT ZONES
        // ===========================================================
        private void PlaceExitZones(LocationGeometryDefinition geometry)
        {
            if (geometry.ExitPrefab == null)
            {
                Debug.LogError("[LocationBuilder] ExitPrefab не назначен.");
                return;
            }

            var exitRoot = geometry.GetOrCreateRoot(geometry.ExitRootName);
            Undo.RegisterFullObjectHierarchyUndo(exitRoot.gameObject, "Place Exit Zones");

            foreach (var exitId in CardinalExits)
            {
                var zoneGO = FindOrCreateExitZone(geometry, exitRoot, exitId);
                ConfigureExitZone(geometry, zoneGO, exitId);
            }

            Debug.Log($"[LocationBuilder] Расставлено {CardinalExits.Length} exit-зон в '{exitRoot.name}'.");
        }

        private GameObject FindOrCreateExitZone(LocationGeometryDefinition geometry, Transform exitRoot, ExitId exitId)
        {
            string goName = $"ExitZone_{exitId}";
            var existing = exitRoot.Find(goName);
            if (existing != null)
                return existing.gameObject;

            var instance = InstantiatePrefab(geometry.ExitPrefab, exitRoot);
            instance.name = goName;
            Undo.RegisterCreatedObjectUndo(instance, "Place Exit Zones");
            return instance;
        }

        private void ConfigureExitZone(LocationGeometryDefinition geometry, GameObject zoneGO, ExitId exitId)
        {
            var locationSize = geometry.LocationSize;
            float halfX = locationSize.x / 2f;
            float halfZ = locationSize.y / 2f;
            float halfHeight = .4f;//geometry.ExitHeight / 2f;
            float thickness = geometry.ExitThickness;
            float halfThickness = thickness / 2f;

            Vector3 localPos;
            Quaternion localRot;
            Vector3 boxSize;

            switch (exitId)
            {
                // BoxCollider.size — в ЛОКАЛЬНЫХ осях объекта. LookRotation(left/right)
                // свопает локальные X/Z относительно мира, поэтому для West/East
                // thickness/длина стороны меняются местами относительно North/South.
                // Зона начинается ровно от края локации и идёт НАРУЖУ на всю толщину.
                case ExitId.ExitWest:
                    localPos = new Vector3(-halfX - halfThickness, halfHeight, 0f);
                    localRot = Quaternion.LookRotation(Vector3.left);
                    boxSize = new Vector3(locationSize.y + thickness * 2, geometry.ExitHeight, thickness);
                    break;
                case ExitId.ExitEast:
                    localPos = new Vector3(halfX + halfThickness, halfHeight, 0f);
                    localRot = Quaternion.LookRotation(Vector3.right);
                    boxSize = new Vector3(locationSize.y + thickness * 2, geometry.ExitHeight, thickness);
                    break;
                case ExitId.ExitNorth:
                    localPos = new Vector3(0f, halfHeight, halfZ + halfThickness);
                    localRot = Quaternion.LookRotation(Vector3.forward);
                    boxSize = new Vector3(locationSize.x, geometry.ExitHeight, thickness);
                    break;
                case ExitId.ExitSouth:
                    localPos = new Vector3(0f, halfHeight, -halfZ - halfThickness);
                    localRot = Quaternion.LookRotation(Vector3.back);
                    boxSize = new Vector3(locationSize.x, geometry.ExitHeight, thickness);
                    break;
                default:
                    Debug.LogWarning($"[LocationBuilder] {exitId} не поддерживается авто-расстановкой.");
                    return;
            }

            Undo.RecordObject(zoneGO.transform, "Place Exit Zones");
            zoneGO.transform.localPosition = localPos;
            zoneGO.transform.localRotation = localRot;

            var box = zoneGO.GetComponent<BoxCollider>();
            if (box == null)
            {
                Debug.LogWarning($"[LocationBuilder] На '{zoneGO.name}' нет BoxCollider — размер не выставлен.");
            }
            else
            {
                Undo.RecordObject(box, "Place Exit Zones");
                box.center = Vector3.zero;
                box.size = boxSize;
            }

            var exitZone = zoneGO.GetComponent<SceneExitZone>();
            if (exitZone == null)
            {
                Debug.LogWarning($"[LocationBuilder] На '{zoneGO.name}' нет SceneExitZone.");
                return;
            }

            var so = new SerializedObject(exitZone);
            var exitIdProp = so.FindProperty("exitId");
            if (exitIdProp != null)
                exitIdProp.enumValueIndex = (int)exitId;
            so.ApplyModifiedProperties();

            exitZone.EditorSyncVisualScale();
            EditorUtility.SetDirty(zoneGO);
        }

        // ===========================================================
        // NAVIGATION (подготовка GridGraph, без Scan)
        // ===========================================================
        private void ConfigureNavigation(LocationGeometryDefinition geometry)
        {
            var navRoot = geometry.GetOrCreateRoot(geometry.NavigationRootName);
            EditorUtility.SetDirty(navRoot.gameObject);

            //LocationNavigationSystem.Configure(geometry.Navigation, geometry.transform.position);
        }

        // ===========================================================
        // COMMON
        // ===========================================================
        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefab) ?? prefab;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);
            if (instance == null)
                instance = Object.Instantiate(prefab, parent);
            return instance;
        }
    }
}