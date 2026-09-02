#if UNITY_EDITOR

using Galactic1;
using Galactic1.Gameplay.Locations;
using Galactic1.RaidLoot.Scene;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AstarMouseButtonScan
{
    static AstarMouseButtonScan()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // button: 0 = ЛКМ, 1 = ПКМ, 2 = СКМ, 3/4 = боковые кнопки (не у всех мышей)
        if (e.type == EventType.MouseDown && e.button == 4)
        {
            var astar = Object.FindFirstObjectByType<AstarPath>(FindObjectsInactive.Include);

            if (astar == null)
            {
                DLog.Alert("AstarPath не найден на сцене.");
                return;
            }

            // Перед сканом выравниваем позиции loot-точек до целых чисел,
            // чтобы они не "плавали" между ячейками грида
            SnapLootSpawnPositions();

            // Графы хранятся сериализованными и обычно десериализуются
            // только при открытии инспектора AstarPath — форсируем вручную.
            if (astar.data.graphs == null || astar.data.graphs.Length == 0)
            {
                astar.data.DeserializeGraphs();
            }

            AstarPath.active = astar; // многие внутренние вызовы Scan полагаются на active
            astar.Scan();

            DLog.Alert("A* grid scanned (mouse button 3).");
            e.Use();
        }

        if (e.type == EventType.MouseDown && e.button == 3)
        {
            var locationRoot = Object.FindFirstObjectByType<SceneContext>(FindObjectsInactive.Include);

            // Выделяем объект — это форсирует OnEnable инспектора,
            // где AstarPathEditor сам вызовет DeserializeGraphs()
            Selection.activeGameObject = locationRoot.gameObject;
        }
    }

    // Округляет x/y позиции всех LootSpawnPoint на сцене до ближайшего целого числа
    // (1.2 -> 1, 22.8 -> 23), z не трогаем.
    static void SnapLootSpawnPositions()
    {
        var lootPoints = Object.FindObjectsByType<LootSpawnPoint>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (lootPoints.Length == 0) return;

        int snappedCount = 0;

        foreach (var lootPoint in lootPoints)
        {
            var t = lootPoint.transform;
            var pos = t.localPosition;
            var rounded = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

            if (rounded == pos) continue;

            Undo.RecordObject(t, "Snap Loot Spawn Position");
            t.localPosition = rounded;
            EditorUtility.SetDirty(t);
            snappedCount++;
        }

        if (snappedCount > 0)
        {
            DLog.Alert($"Snapped {snappedCount} loot spawn point(s) to whole numbers.");
        }
    }
}

#endif