#if UNITY_EDITOR
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Authoring;
using Galactic1.Configs.Enemies;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Scene;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring.Editor
{
    [CustomEditor(typeof(LocationZonesEditorTool))]
    public class LocationZonesEditorToolEditor : UnityEditor.Editor
    {
        private LocationZonesEditorTool _tool;

        // Editor-only состояние свёрнутости зон и блоков loot/zombie, не сериализуется в сцену/префаб
        private readonly Dictionary<Transform, bool> _expandedZones = new Dictionary<Transform, bool>();
        private readonly Dictionary<Transform, bool> _expandedSpawnGroups = new Dictionary<Transform, bool>();

        // Editor-only ввод для следующего создаваемого loot/zombie объекта, ключ - spawnParent конкретной зоны
        private readonly Dictionary<Transform, string> _pendingName = new Dictionary<Transform, string>();
        private readonly Dictionary<Transform, Vector3> _pendingPosition = new Dictionary<Transform, Vector3>();
        private readonly Dictionary<Transform, LootContainerDefinitionConfig> _pendingLootConfig = new Dictionary<Transform, LootContainerDefinitionConfig>();
        private readonly Dictionary<Transform, EnemyGroupConfig> _pendingEnemyGroup = new Dictionary<Transform, EnemyGroupConfig>();
        private readonly Dictionary<Transform, float> _pendingWanderRadius = new Dictionary<Transform, float>();

        private void OnEnable()
        {
            _tool = (LocationZonesEditorTool)target;
        }

        private void OnSceneGUI()
        {
            var nameStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            var entryStyle = new GUIStyle
            {
                normal = { textColor = new Color(1f, 0.82f, 0.4f) },
                alignment = TextAnchor.MiddleCenter
            };

            foreach (var zone in GetZones())
            {
                DrawLootLabels(zone, nameStyle, entryStyle);
                DrawZombieLabels(zone, nameStyle, entryStyle);
            }
        }

        // подпись имени над каждой loot-точкой + отдельный столбец под нижней границей зоны:
        // одна строка на каждую точку, формат "LootName_containerName"
        private void DrawLootLabels(Transform zone, GUIStyle nameStyle, GUIStyle entryStyle)
        {
            var lootSpawn = zone.Find(LocationZonesEditorTool.LootSpawnName);
            if (lootSpawn == null) return;

            const float nameOffset = 0.4f;

            // подпись имени над самой точкой - как у zombie spawn point
            for (int i = 0; i < lootSpawn.childCount; i++)
            {
                var point = lootSpawn.GetChild(i);
                float pointHandleSize = HandleUtility.GetHandleSize(point.position);
                Handles.Label(point.position + Vector3.up * (nameOffset * pointHandleSize), point.name, nameStyle);
            }

            var gameplayZone = zone.GetComponent<GameplayZone>();
            if (gameplayZone == null) return;

            const float margin = 0.5f;
            const float lineHeight = 0.35f;

            Vector3 anchor = gameplayZone.GetBottomBorderWorldPosition();
            // GetHandleSize растёт пропорционально расстоянию до камеры - так столбец не сжимается при отдалении
            float handleSize = HandleUtility.GetHandleSize(anchor);

            float y = margin;
            for (int i = 0; i < lootSpawn.childCount; i++)
            {
                var point = lootSpawn.GetChild(i);

                var lootSpawnPoint = point.GetComponent<LootSpawnPoint>();
                var containerId = lootSpawnPoint != null ? lootSpawnPoint.Config?.Id : null;

                string containerName = containerId != null
                    ? (!string.IsNullOrEmpty(containerId.DebugKey) ? containerId.DebugKey : containerId.name)
                    : "—";

                Handles.Label(anchor + Vector3.up * (y * handleSize), $"{point.name}_{containerName}", entryStyle);
                y -= lineHeight;
            }
        }

        // над каждой точкой EnemySpawnPoint - имя точки, а под ним столбцом тип зомби x количество из EnemyGroupConfig
        private void DrawZombieLabels(Transform zone, GUIStyle nameStyle, GUIStyle entryStyle)
        {
            var zombieSpawn = zone.Find(LocationZonesEditorTool.ZombieSpawnName);
            if (zombieSpawn == null) return;

            const float lineHeight = 0.35f;
            const float nameOffset = 0.4f;

            for (int i = 0; i < zombieSpawn.childCount; i++)
            {
                var point = zombieSpawn.GetChild(i);
                float handleSize = HandleUtility.GetHandleSize(point.position);

                Handles.Label(point.position + Vector3.up * (nameOffset * handleSize), point.name, nameStyle);

                var spawnPoint = point.GetComponent<EnemySpawnPoint>();
                var entries = spawnPoint != null ? spawnPoint.Group?.Enemies : null;
                if (entries == null) continue;

                float y = nameOffset + lineHeight;
                foreach (var entry in entries)
                {
                    if (entry == null) continue;

                    string enemyName = entry.Enemy != null
                        ? (!string.IsNullOrEmpty(entry.Enemy.DisplayName) ? entry.Enemy.DisplayName : entry.Enemy.name)
                        : "—";

                    Handles.Label(point.position + Vector3.up * (y * handleSize), $"{enemyName} x{entry.Count}", entryStyle);
                    y += lineHeight;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Zones", EditorStyles.boldLabel);

            if (_tool.ZonePrefab != null && _tool.ZonePrefab.GetComponent<GameplayZone>() == null)
            {
                EditorGUILayout.HelpBox("Zone Prefab не содержит компонент GameplayZone", MessageType.Warning);
            }

            if (GUILayout.Button("+ Create Zone"))
            {
                CreateZone();
            }

            EditorGUILayout.Space(5);

            foreach (var zone in GetZones())
            {
                DrawZone(zone);
            }
        }

        // ---------- Zones ----------

        private List<Transform> GetZones()
        {
            var parent = _tool.ZoneRootParent;
            var list = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                // зона определяется по наличию GameplayZone, а не по имени - имя можно менять свободно
                if (child.GetComponent<GameplayZone>() != null)
                    list.Add(child);
            }
            return list;
        }

        // Общий счётчик loot/zombie точек по ВСЕМ зонам - используется для генерации имени
        // следующего создаваемого объекта, чтобы нумерация была сквозной, а не локальной для зоны
        private int GetGlobalSpawnCount(string spawnChildName)
        {
            int count = 1;
            foreach (var zone in GetZones())
            {
                var spawnParent = zone.Find(spawnChildName);
                if (spawnParent != null)
                    count += spawnParent.childCount;
            }
            return count;
        }

        private void CreateZone()
        {
            var parent = _tool.ZoneRootParent;
            int index = GetZones().Count;

            GameObject zoneGo;
            if (_tool.ZonePrefab != null)
            {
                zoneGo = PrefabUtility.InstantiatePrefab(_tool.ZonePrefab, parent) as GameObject;
                Undo.RegisterCreatedObjectUndo(zoneGo, "Create Zone");
            }
            else
            {
                zoneGo = new GameObject();
                Undo.RegisterCreatedObjectUndo(zoneGo, "Create Zone");
                zoneGo.transform.SetParent(parent, false);
                Undo.AddComponent<GameplayZone>(zoneGo);
            }

            zoneGo.name = $"{LocationZonesEditorTool.ZonePrefix}{index:00}";

            EnsureChild(zoneGo.transform, LocationZonesEditorTool.LootSpawnName);
            EnsureChild(zoneGo.transform, LocationZonesEditorTool.ZombieSpawnName);

            _expandedZones[zoneGo.transform] = true;
            EditorUtility.SetDirty(_tool);
        }

        private void EnsureChild(Transform parent, string childName)
        {
            // на случай если zonePrefab уже содержит loot_spawn/zombie_spawn - не дублируем
            if (parent.Find(childName) != null) return;

            var go = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(go, "Create Zone");
            go.transform.SetParent(parent, false);
        }

        private void DeleteZone(Transform zone)
        {
            _expandedZones.Remove(zone);

            var lootSpawn = zone.Find(LocationZonesEditorTool.LootSpawnName);
            var zombieSpawn = zone.Find(LocationZonesEditorTool.ZombieSpawnName);
            if (lootSpawn != null) _expandedSpawnGroups.Remove(lootSpawn);
            if (zombieSpawn != null) _expandedSpawnGroups.Remove(zombieSpawn);

            Undo.DestroyObjectImmediate(zone.gameObject);
        }

        private void DrawZone(Transform zone)
        {
            if (zone == null) return; // могла быть удалена в этом же кадре

            if (!_expandedZones.ContainsKey(zone))
                _expandedZones[zone] = false;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            bool expanded = _expandedZones[zone];
            if (GUILayout.Button(expanded ? "▼" : "▶", EditorStyles.label, GUILayout.Width(14)))
            {
                expanded = !expanded;
            }
            _expandedZones[zone] = expanded;

            // Переименование зоны прямо в заголовке
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.DelayedTextField(zone.name);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newName))
            {
                Undo.RecordObject(zone.gameObject, "Rename Zone");
                zone.gameObject.name = newName;
            }

            GUILayout.FlexibleSpace();
            bool deletePressed = GUILayout.Button("Delete", GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            if (deletePressed)
            {
                EditorGUILayout.EndVertical();
                DeleteZone(zone);
                return;
            }

            if (expanded)
            {
                EditorGUI.indentLevel++;

                DrawZoneBounds(zone);

                EditorGUILayout.Space(4);

                var lootSpawn = zone.Find(LocationZonesEditorTool.LootSpawnName);
                var zombieSpawn = zone.Find(LocationZonesEditorTool.ZombieSpawnName);

                DrawLootSpawnGroup(lootSpawn, _tool.LootPrefab);
                EditorGUILayout.Space(4);
                DrawZombieSpawnGroup(zombieSpawn, _tool.ZombiePrefab);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        // ---------- GameplayZone bounds (zoneCenter / size) ----------

        private void DrawZoneBounds(Transform zone)
        {
            var gameplayZone = zone.GetComponent<GameplayZone>();
            if (gameplayZone == null)
            {
                EditorGUILayout.HelpBox("На зоне нет компонента GameplayZone", MessageType.Warning);
                return;
            }

            var so = new SerializedObject(gameplayZone);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("zoneCenter"));
            EditorGUILayout.PropertyField(so.FindProperty("size"));

            so.ApplyModifiedProperties();
        }

        // ---------- Общие для loot/zombie: заголовок группы, переименование, позиция ----------

        private bool DrawSpawnGroupHeader(string label, Transform spawnParent)
        {
            if (!_expandedSpawnGroups.ContainsKey(spawnParent))
                _expandedSpawnGroups[spawnParent] = false;

            bool expanded = EditorGUILayout.Foldout(
                _expandedSpawnGroups[spawnParent],
                $"{label} points ({spawnParent.childCount})",
                true);
            _expandedSpawnGroups[spawnParent] = expanded;
            return expanded;
        }

        private void DrawRenameAndPosition(Transform point, out bool removePressed)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            string newPointName = EditorGUILayout.DelayedTextField(point.name);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newPointName))
            {
                Undo.RecordObject(point.gameObject, "Rename Spawn Point");
                point.gameObject.name = newPointName;
            }

            removePressed = GUILayout.Button("-", GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = EditorGUILayout.Vector3Field(GUIContent.none, point.localPosition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(point, "Move Spawn Point");
                point.localPosition = newPos;
            }
        }

        private GameObject InstantiateSpawnObject(Transform spawnParent, GameObject prefab, string undoLabel, string objectName, Vector3 localPosition)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, spawnParent) as GameObject;
            if (instance == null) return null;

            Undo.RegisterCreatedObjectUndo(instance, undoLabel);
            instance.name = string.IsNullOrWhiteSpace(objectName) ? instance.name : objectName;
            instance.transform.localPosition = localPosition;
            return instance;
        }

        // ---------- Loot spawn (LootSpawnPoint + LootContainerDefinitionConfig) ----------

        private void DrawLootSpawnGroup(Transform spawnParent, GameObject prefab)
        {
            if (spawnParent == null)
            {
                EditorGUILayout.HelpBox("Loot spawn parent не найден", MessageType.Warning);
                return;
            }

            if (!DrawSpawnGroupHeader("Loot", spawnParent)) return;

            EditorGUI.indentLevel++;

            Transform toRemove = null;
            for (int i = 0; i < spawnParent.childCount; i++)
            {
                var point = spawnParent.GetChild(i);

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUI.backgroundColor = prevColor;

                DrawRenameAndPosition(point, out bool removePressed);
                if (removePressed) toRemove = point;

                var lootSpawnPoint = point.GetComponent<LootSpawnPoint>();
                if (lootSpawnPoint != null)
                {
                    var so = new SerializedObject(lootSpawnPoint);
                    so.Update();
                    EditorGUILayout.PropertyField(so.FindProperty("_config"), new GUIContent("Loot Config"));
                    so.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.HelpBox("Нет компонента LootSpawnPoint", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }

            if (toRemove != null)
            {
                Undo.DestroyObjectImmediate(toRemove.gameObject);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.Space(4);

            if (!_pendingName.ContainsKey(spawnParent))
                _pendingName[spawnParent] = $"LC{GetGlobalSpawnCount(LocationZonesEditorTool.LootSpawnName):00}";
            if (!_pendingPosition.ContainsKey(spawnParent))
                _pendingPosition[spawnParent] = Vector3.zero;
            if (!_pendingLootConfig.ContainsKey(spawnParent))
                _pendingLootConfig[spawnParent] = null;

            _pendingName[spawnParent] = EditorGUILayout.TextField("New name", _pendingName[spawnParent]);
            _pendingPosition[spawnParent] = EditorGUILayout.Vector3Field("New position", _pendingPosition[spawnParent]);
            _pendingLootConfig[spawnParent] = (LootContainerDefinitionConfig)EditorGUILayout.ObjectField(
                "New loot config", _pendingLootConfig[spawnParent], typeof(LootContainerDefinitionConfig), false);

            using (new EditorGUI.DisabledScope(prefab == null))
            {
                if (GUILayout.Button("+ Add Loot"))
                {
                    var instance = InstantiateSpawnObject(spawnParent, prefab, "Add Loot", _pendingName[spawnParent], _pendingPosition[spawnParent]);
                    if (instance != null)
                    {
                        var lootSpawnPoint = instance.GetComponent<LootSpawnPoint>();
                        if (lootSpawnPoint != null && _pendingLootConfig[spawnParent] != null)
                        {
                            var so = new SerializedObject(lootSpawnPoint);
                            so.Update();
                            so.FindProperty("_config").objectReferenceValue = _pendingLootConfig[spawnParent];
                            so.ApplyModifiedProperties();
                        }
                    }
                    // Счётчик глобальный по всем зонам - берём его уже ПОСЛЕ добавления новой точки
                    _pendingName[spawnParent] = $"LC{GetGlobalSpawnCount(LocationZonesEditorTool.LootSpawnName):00}";
                    // Структурное изменение иерархии (добавили child) делает текущий проход OnGUI
                    // несовместимым с новым состоянием - прерываем его, чтобы поле сразу показало новое имя
                    GUI.FocusControl(null);
                    Repaint();
                    GUIUtility.ExitGUI();
                }
            }

            if (prefab == null)
            {
                EditorGUILayout.HelpBox("Loot prefab не задан в инспекторе", MessageType.None);
            }

            EditorGUI.indentLevel--;
        }

        // ---------- Zombie spawn (EnemySpawnPoint + EnemyGroupConfig) ----------

        private void DrawZombieSpawnGroup(Transform spawnParent, GameObject prefab)
        {
            if (spawnParent == null)
            {
                EditorGUILayout.HelpBox("Zombie spawn parent не найден", MessageType.Warning);
                return;
            }

            if (!DrawSpawnGroupHeader("Zombie", spawnParent)) return;

            EditorGUI.indentLevel++;

            Transform toRemove = null;
            for (int i = 0; i < spawnParent.childCount; i++)
            {
                var point = spawnParent.GetChild(i);

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUI.backgroundColor = prevColor;

                DrawRenameAndPosition(point, out bool removePressed);
                if (removePressed) toRemove = point;

                var enemySpawnPoint = point.GetComponent<EnemySpawnPoint>();
                if (enemySpawnPoint != null)
                {
                    // Как и Loot Config выше - редактируем через SerializedObject, а не через прямое
                    // присваивание свойству. Прямое присваивание (enemySpawnPoint.Group = ...) не всегда
                    // корректно фиксирует изменение как override инстанса префаба и не гарантированно
                    // помечает объект dirty, из-за чего правки могли не сохраняться в сцену/префаб.
                    var so = new SerializedObject(enemySpawnPoint);
                    so.Update();

                    var groupProp = so.FindProperty("Group");
                    var wanderProp = so.FindProperty("WanderRadius");

                    if (groupProp != null)
                        EditorGUILayout.PropertyField(groupProp, new GUIContent("Enemy Group"));
                    else
                        EditorGUILayout.HelpBox("Не найдено сериализуемое поле для Group (проверь имя приватного поля в EnemySpawnPoint)", MessageType.Warning);

                    if (wanderProp != null)
                        EditorGUILayout.PropertyField(wanderProp, new GUIContent("Wander Radius"));
                    else
                        EditorGUILayout.HelpBox("Не найдено сериализуемое поле для WanderRadius (проверь имя приватного поля в EnemySpawnPoint)", MessageType.Warning);

                    so.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.HelpBox("Нет компонента EnemySpawnPoint", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }

            if (toRemove != null)
            {
                Undo.DestroyObjectImmediate(toRemove.gameObject);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.Space(4);

            if (!_pendingName.ContainsKey(spawnParent))
                _pendingName[spawnParent] = $"G{GetGlobalSpawnCount(LocationZonesEditorTool.ZombieSpawnName):00}";
            if (!_pendingPosition.ContainsKey(spawnParent))
                _pendingPosition[spawnParent] = Vector3.zero;
            if (!_pendingEnemyGroup.ContainsKey(spawnParent))
                _pendingEnemyGroup[spawnParent] = null;
            if (!_pendingWanderRadius.ContainsKey(spawnParent))
                _pendingWanderRadius[spawnParent] = 5f;

            _pendingName[spawnParent] = EditorGUILayout.TextField("New name", _pendingName[spawnParent]);
            _pendingPosition[spawnParent] = EditorGUILayout.Vector3Field("New position", _pendingPosition[spawnParent]);
            _pendingEnemyGroup[spawnParent] = (EnemyGroupConfig)EditorGUILayout.ObjectField(
                "New enemy group", _pendingEnemyGroup[spawnParent], typeof(EnemyGroupConfig), false);
            _pendingWanderRadius[spawnParent] = EditorGUILayout.FloatField("New wander radius", _pendingWanderRadius[spawnParent]);

            using (new EditorGUI.DisabledScope(prefab == null))
            {
                if (GUILayout.Button("+ Add Zombie"))
                {
                    var instance = InstantiateSpawnObject(spawnParent, prefab, "Add Zombie", _pendingName[spawnParent], _pendingPosition[spawnParent]);
                    if (instance != null)
                    {
                        var enemySpawnPoint = instance.GetComponent<EnemySpawnPoint>();
                        if (enemySpawnPoint != null)
                        {
                            var so = new SerializedObject(enemySpawnPoint);
                            so.Update();

                            var groupProp = so.FindProperty("Group");
                            var wanderProp = so.FindProperty("WanderRadius");

                            if (groupProp != null)
                                groupProp.objectReferenceValue = _pendingEnemyGroup[spawnParent];
                            if (wanderProp != null)
                                wanderProp.floatValue = _pendingWanderRadius[spawnParent];

                            so.ApplyModifiedProperties();
                        }
                    }
                    // Счётчик глобальный по всем зонам - берём его уже ПОСЛЕ добавления новой точки
                    _pendingName[spawnParent] = $"G{GetGlobalSpawnCount(LocationZonesEditorTool.ZombieSpawnName):00}";
                    // Структурное изменение иерархии (добавили child) делает текущий проход OnGUI
                    // несовместимым с новым состоянием - прерываем его, чтобы поле сразу показало новое имя
                    GUI.FocusControl(null);
                    Repaint();
                    GUIUtility.ExitGUI();
                }
            }

            if (prefab == null)
            {
                EditorGUILayout.HelpBox("Zombie prefab не задан в инспекторе", MessageType.None);
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif