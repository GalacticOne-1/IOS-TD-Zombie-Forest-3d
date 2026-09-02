using System.Collections.Generic;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools.RaidLoot
{
    public class LootTableManagerWindow : EditorWindow
    {
        [MenuItem("Tools/Items/Loot Table Manager")]
        public static void Open() => GetWindow<LootTableManagerWindow>("Loot Tables");

        private List<LootTableConfig> _allTables = new();
        private LootTableConfig _selected;
        private SerializedObject _selectedSO;
        private LootTableConfig _pendingDelete;

        private Vector2 _leftScroll;
        private Vector2 _rightScroll;

        private string _search = "";
        private const float LeftWidth = 220f;

        private readonly List<bool> _slotFoldouts = new();
        private bool _guaranteedFoldout = true;
        private bool _slotsSectionFoldout = true;

        private GUIStyle _selectedRowStyle;
        private GUIStyle _normalRowStyle;
        private GUIStyle _sectionBoxStyle;
        private GUIStyle _slotBoxStyle;
        private GUIStyle _tagStyle;
        private GUIStyle _headerLabelStyle;
        private bool _stylesReady;

        private static readonly Color ColGuaranteed = new(0.20f, 0.80f, 0.35f, 0.18f);
        private static readonly Color ColSlot = new(0.25f, 0.50f, 0.90f, 0.15f);
        private static readonly Color ColWeightBar = new(0.30f, 0.70f, 1.00f, 0.80f);
        private static readonly Color ColGBar = new(0.20f, 0.85f, 0.40f, 0.80f);
        private static readonly Color ColSelected = new(0.25f, 0.50f, 0.90f, 0.35f);
        private static readonly Color ColHover = new(1f, 1f, 1f, 0.06f);

        private void OnEnable() => Refresh();
        private void OnFocus() => Refresh();

        private void Refresh()
        {
            _allTables.Clear();
            var guids = AssetDatabase.FindAssets("t:LootTableConfig");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var cfg = AssetDatabase.LoadAssetAtPath<LootTableConfig>(path);
                if (cfg != null) _allTables.Add(cfg);
            }
        }

        private const float BottomBarHeight = 22f;

        private void OnGUI()
        {
            EnsureStyles();

            if (_showCreateDialog)
            {
                float dw = 380f, dh = 160f;
                var dialogRc = new Rect(
                    (position.width - dw) * 0.5f,
                    (position.height - dh) * 0.5f,
                    dw, dh);

                var ev = Event.current;
                if (ev.type != EventType.Layout && ev.type != EventType.Repaint)
                {
                    if (!dialogRc.Contains(ev.mousePosition))
                        ev.Use();
                }
            }

            float mainHeight = position.height - BottomBarHeight;
            EditorGUILayout.BeginHorizontal(GUILayout.Height(mainHeight));
            DrawLeftPanel(mainHeight);
            DrawDivider(mainHeight);
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
            DrawBottomBar();

            DrawCreateDialog();

            if (_pendingDelete != null)
            {
                DeleteTable(_pendingDelete);
                _pendingDelete = null;
                GUIUtility.ExitGUI();
            }
        }

        // ── Left panel ────────────────────────────────────────────────────────

        private void DrawLeftPanel(float height)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LeftWidth), GUILayout.Height(height));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"Tables ({_allTables.Count})", EditorStyles.boldLabel);
            if (GUILayout.Button("⟳", EditorStyles.toolbarButton, GUILayout.Width(24)))
                Refresh();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🔍", GUILayout.Width(18));
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(_search) &&
                GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(18)))
                _search = "";
            EditorGUILayout.EndHorizontal();

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, GUILayout.ExpandHeight(true));

            foreach (var tbl in _allTables)
            {
                if (tbl == null) continue;

                string label = tbl.name;
                if (!string.IsNullOrEmpty(_search) &&
                    !label.ToLower().Contains(_search.ToLower())) continue;

                bool isSel = tbl == _selected;
                var style = isSel ? _selectedRowStyle : _normalRowStyle;

                var rc = GUILayoutUtility.GetRect(LeftWidth - 8, 28);

                if (!isSel && rc.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(rc, ColHover);
                    Repaint();
                }

                if (isSel) EditorGUI.DrawRect(rc, ColSelected);

                var iconRc = new Rect(rc.x + 4, rc.y + 4, 20, 20);
                EditorGUI.DrawRect(iconRc, new Color(0.3f, 0.6f, 1f, 0.3f));
                GUI.Label(iconRc, "📋", EditorStyles.centeredGreyMiniLabel);

                var textRc = new Rect(rc.x + 28, rc.y + 5, rc.width - 56, 18);
                GUI.Label(textRc, label, isSel ? EditorStyles.whiteLabel : EditorStyles.label);

                int slotCount = tbl.Slots?.Length ?? 0;
                var badgeRc = new Rect(rc.xMax - 26, rc.y + 6, 22, 16);
                EditorGUI.DrawRect(badgeRc, new Color(0.3f, 0.5f, 0.9f, 0.4f));
                GUI.Label(badgeRc, $"{slotCount}", EditorStyles.centeredGreyMiniLabel);

                if (GUI.Button(rc, GUIContent.none, GUIStyle.none))
                    SelectTable(tbl);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SelectTable(LootTableConfig tbl)
        {
            if (_selected == tbl) return;
            _selected = tbl;
            _selectedSO = _selected != null ? new SerializedObject(_selected) : null;
            _slotFoldouts.Clear();
            if (tbl?.Slots != null)
                for (int i = 0; i < tbl.Slots.Length; i++)
                    _slotFoldouts.Add(true);
            GUI.FocusControl(null);
        }

        // ── Divider ───────────────────────────────────────────────────────────

        private void DrawDivider(float height)
        {
            var rc = GUILayoutUtility.GetRect(2, height, GUILayout.Width(2));
            EditorGUI.DrawRect(rc, new Color(0f, 0f, 0f, 0.25f));
        }

        // ── Right panel ───────────────────────────────────────────────────────

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (_selected == null || _selectedSO == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("← Select a Loot Table",
                    EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                return;
            }

            _selectedSO.Update();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(_selected.name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(42)))
                EditorGUIUtility.PingObject(_selected);
            if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(52)))
                Selection.activeObject = _selected;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.38f, 0.38f);
            if (GUILayout.Button("🗑 Delete", EditorStyles.toolbarButton, GUILayout.Width(64)))
            {
                if (EditorUtility.DisplayDialog(
                        "Delete Loot Table",
                        $"Delete '{_selected.name}' and its ID asset?\nThis cannot be undone.",
                        "Delete", "Cancel"))
                {
                    _pendingDelete = _selected;
                }
            }

            GUI.backgroundColor = prevBg;

            EditorGUILayout.EndHorizontal();

            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
            EditorGUILayout.Space(6);

            DrawTableHeader();
            EditorGUILayout.Space(6);
            DrawStatsBar();
            EditorGUILayout.Space(8);
            DrawGuaranteedSection();
            EditorGUILayout.Space(8);
            DrawSlotsSection();
            EditorGUILayout.Space(12);

            EditorGUILayout.EndScrollView();

            if (_selectedSO.ApplyModifiedProperties())
            {
                var slots = _selectedSO.FindProperty("_slots");
                while (_slotFoldouts.Count < slots.arraySize) _slotFoldouts.Add(true);
                while (_slotFoldouts.Count > slots.arraySize && _slotFoldouts.Count > 0)
                    _slotFoldouts.RemoveAt(_slotFoldouts.Count - 1);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTableHeader()
        {
            var idProp = _selectedSO.FindProperty("_id");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table ID", GUILayout.Width(68));
            EditorGUILayout.PropertyField(idProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        // ── Stats bar ─────────────────────────────────────────────────────────

        private void DrawStatsBar()
        {
            var slotsProp = _selectedSO.FindProperty("_slots");
            var guaranteedProp = _selectedSO.FindProperty("_guaranteedEntries");

            // Pool entries now come from SharedPool references, count unique + total
            int totalPoolEntries = 0;
            int slotsWithoutPool = 0;
            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                var sharedPoolProp = slotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("SharedPool");
                var poolCfg = sharedPoolProp?.objectReferenceValue as LootPoolConfig;
                if (poolCfg != null && poolCfg.Pool != null)
                    totalPoolEntries += poolCfg.Pool.Length;
                else
                    slotsWithoutPool++;
            }

            EditorGUILayout.BeginHorizontal();
            Chip($"Slots: {slotsProp.arraySize}", new Color(0.3f, 0.6f, 1.0f));
            Chip($"Pool entries: {totalPoolEntries}", new Color(0.4f, 0.8f, 0.4f));
            Chip($"Guaranteed: {guaranteedProp.arraySize}", new Color(0.9f, 0.7f, 0.2f));
            if (slotsWithoutPool > 0)
                Chip($"No pool: {slotsWithoutPool}", new Color(0.9f, 0.3f, 0.3f));
            EditorGUILayout.EndHorizontal();
        }

        private void Chip(string text, Color color)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = color * 0.55f;
            GUILayout.Box(text, EditorStyles.miniButton, GUILayout.Height(20));
            GUI.backgroundColor = prev;
        }

        // ── Guaranteed section ────────────────────────────────────────────────

        private void DrawGuaranteedSection()
        {
            var prop = _selectedSO.FindProperty("_guaranteedEntries");

            ColorBox(ColGuaranteed, () =>
            {
                EditorGUILayout.BeginHorizontal();

                var foldoutRect = GUILayoutUtility.GetRect(
                    GUIContent.none, EditorStyles.foldoutHeader,
                    GUILayout.ExpandWidth(false), GUILayout.MinWidth(160));
                _guaranteedFoldout = EditorGUI.Foldout(
                    foldoutRect, _guaranteedFoldout,
                    $"✅  Guaranteed  [{prop.arraySize}]", true, EditorStyles.foldoutHeader);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("＋", EditorStyles.miniButton, GUILayout.Width(22)))
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                EditorGUILayout.EndHorizontal();

                if (!_guaranteedFoldout) return;
                EditorGUILayout.Space(4);

                if (prop.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("Drops every open — ignores budget & RNG.", MessageType.Info);
                    return;
                }

                int del = -1;
                for (int i = 0; i < prop.arraySize; i++)
                    del = DrawGuaranteedRow(prop.GetArrayElementAtIndex(i), i, del);
                if (del >= 0) prop.DeleteArrayElementAtIndex(del);
            });
        }

        private int DrawGuaranteedRow(SerializedProperty e, int idx, int del)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            DrawIcon(e.FindPropertyRelative("_item"), 30f);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(e.FindPropertyRelative("_item"),
                GUIContent.none, GUILayout.MinWidth(60), GUILayout.ExpandWidth(true));

            GUILayout.Label("Amt", EditorStyles.miniLabel, GUILayout.Width(28));
            var minAmtP = e.FindPropertyRelative("_minAmount");
            var maxAmtP = e.FindPropertyRelative("_maxAmount");
            EditorGUILayout.PropertyField(minAmtP, GUIContent.none, GUILayout.Width(40));
            GUILayout.Label("–", GUILayout.Width(9));
            EditorGUILayout.PropertyField(maxAmtP, GUIContent.none, GUILayout.Width(40));

            GUILayout.Label("Dur%", EditorStyles.miniLabel, GUILayout.Width(28));
            var durProp = e.FindPropertyRelative("_durabilityPercent");
            durProp.intValue = Mathf.Clamp(
                EditorGUILayout.IntField(durProp.intValue, GUILayout.Width(40)), 0, 100);

            EditorGUILayout.EndHorizontal();

            // Warn if min > max (RollAmount fail-safe returns min, but signal it in editor)
            if (minAmtP.intValue > maxAmtP.intValue)
                EditorGUILayout.LabelField("⚠ Min > Max — RollAmount will return Min",
                    EditorStyles.centeredGreyMiniLabel);
            else
                EditorGUILayout.LabelField("Always drops · no budget · no RNG", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = new Color(1f, 0.38f, 0.38f);
            if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(20)))
                del = idx;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
            return del;
        }

        // ── Slots section ─────────────────────────────────────────────────────

        private void DrawSlotsSection()
        {
            var prop = _selectedSO.FindProperty("_slots");

            while (_slotFoldouts.Count < prop.arraySize) _slotFoldouts.Add(true);
            while (_slotFoldouts.Count > prop.arraySize && _slotFoldouts.Count > 0)
                _slotFoldouts.RemoveAt(_slotFoldouts.Count - 1);

            ColorBox(ColSlot, () =>
            {
                EditorGUILayout.BeginHorizontal();

                var foldoutRect = GUILayoutUtility.GetRect(
                    GUIContent.none, EditorStyles.foldoutHeader,
                    GUILayout.ExpandWidth(false), GUILayout.MinWidth(160));
                _slotsSectionFoldout = EditorGUI.Foldout(
                    foldoutRect, _slotsSectionFoldout,
                    $"🎲  Slots  [{prop.arraySize}]", true, EditorStyles.foldoutHeader);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("▼ all", EditorStyles.miniButton, GUILayout.Width(44)))
                    for (int k = 0; k < _slotFoldouts.Count; k++)
                        _slotFoldouts[k] = true;
                if (GUILayout.Button("▲ all", EditorStyles.miniButton, GUILayout.Width(44)))
                    for (int k = 0; k < _slotFoldouts.Count; k++)
                        _slotFoldouts[k] = false;
                if (GUILayout.Button("＋ Slot", EditorStyles.miniButton, GUILayout.Width(56)))
                {
                    prop.InsertArrayElementAtIndex(prop.arraySize);

                    // InsertArrayElementAtIndex clones the *previous* element's serialized
                    // data (including object references). Without resetting these, the new
                    // slot ends up pointing at the SAME LootPoolConfig asset as the slot it
                    // was cloned from — so editing the pool in either slot edits the shared
                    // asset and looks like "changing one slot changes all slots".
                    var newSlot = prop.GetArrayElementAtIndex(prop.arraySize - 1);

                    var slotIdP = newSlot.FindPropertyRelative("SlotId");
                    if (slotIdP != null) slotIdP.stringValue = "";

                    var sharedPoolP = newSlot.FindPropertyRelative("SharedPool");
                    if (sharedPoolP != null) sharedPoolP.objectReferenceValue = null;

                    var guarP = newSlot.FindPropertyRelative("IsGuaranteed");
                    if (guarP != null) guarP.boolValue = false;

                    var repeatP = newSlot.FindPropertyRelative("RepeatCount");
                    if (repeatP != null && repeatP.intValue < 1) repeatP.intValue = 1;

                    _slotFoldouts.Add(true);
                }

                EditorGUILayout.EndHorizontal();

                if (!_slotsSectionFoldout) return;
                EditorGUILayout.Space(4);

                if (prop.arraySize == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Each slot rolls RepeatCount times. If ActivationChance passes, one item is picked from SharedPool.",
                        MessageType.Info);
                    return;
                }

                int del = -1;
                for (int i = 0; i < prop.arraySize; i++)
                {
                    if (DrawSlot(prop.GetArrayElementAtIndex(i), i))
                        del = i;
                    GUILayout.Space(4);
                }

                if (del >= 0)
                {
                    prop.DeleteArrayElementAtIndex(del);
                    if (del < _slotFoldouts.Count) _slotFoldouts.RemoveAt(del);
                }
            });
        }

        // returns true → delete
        private bool DrawSlot(SerializedProperty slot, int idx)
        {
            var slotIdProp = slot.FindPropertyRelative("SlotId");
            var guaranteedP = slot.FindPropertyRelative("IsGuaranteed");
            var activationP = slot.FindPropertyRelative("ActivationChance");
            var minTierP = slot.FindPropertyRelative("MinTier");
            var maxTierP = slot.FindPropertyRelative("MaxTier");
            var repeatP = slot.FindPropertyRelative("RepeatCount");
            var sharedPoolP = slot.FindPropertyRelative("SharedPool");

            bool isGuar = guaranteedP.boolValue;
            float actPct = activationP.floatValue * 100f;
            string name = string.IsNullOrEmpty(slotIdProp.stringValue)
                ? $"Slot {idx + 1}"
                : slotIdProp.stringValue;

            var poolCfg = sharedPoolP.objectReferenceValue as LootPoolConfig;
            int poolCount = poolCfg?.Pool?.Length ?? 0;

            bool deleted = false;
            ColorBox(isGuar ? ColGuaranteed : new Color(0.2f, 0.2f, 0.2f, 0.25f), () =>
            {
                // ── Slot header ───────────────────────────────────────────────
                EditorGUILayout.BeginHorizontal();

                _slotFoldouts[idx] = EditorGUILayout.Foldout(
                    _slotFoldouts[idx], $" #{idx + 1}  {name}", true, EditorStyles.foldoutHeader);

                GUILayout.FlexibleSpace();

                if (isGuar)
                    GUILayout.Label("GUAR", _tagStyle, GUILayout.Width(36));

                GUILayout.Label($"x{repeatP.intValue}", EditorStyles.miniLabel, GUILayout.Width(24));
                GUILayout.Label($"{actPct:F0}%", EditorStyles.miniLabel, GUILayout.Width(30));
                GUILayout.Label($"pool:{poolCount}", EditorStyles.miniLabel, GUILayout.Width(46));

                if (poolCfg == null)
                    GUILayout.Label("⚠", _tagStyle, GUILayout.Width(16));

                // Move up / down
                if (idx > 0 && GUILayout.Button("↑", EditorStyles.miniButton, GUILayout.Width(18)))
                    MoveSlot(idx, idx - 1);
                if (idx < _selectedSO.FindProperty("_slots").arraySize - 1 &&
                    GUILayout.Button("↓", EditorStyles.miniButton, GUILayout.Width(18)))
                    MoveSlot(idx, idx + 1);

                GUI.backgroundColor = new Color(1f, 0.38f, 0.38f);
                if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20)))
                    deleted = true;
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                if (!_slotFoldouts[idx]) return;
                EditorGUILayout.Space(4);

                // ── Settings ──────────────────────────────────────────────────
                EditorGUILayout.BeginHorizontal();
                LabelField("Slot ID", 62);
                EditorGUILayout.PropertyField(slotIdProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                LabelField("Guaranteed", 62);
                EditorGUILayout.PropertyField(guaranteedP, GUIContent.none, GUILayout.Width(18));
                GUILayout.Label("ignores budget & activation", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                using (new EditorGUI.DisabledScope(guaranteedP.boolValue))
                {
                    EditorGUILayout.BeginHorizontal();
                    LabelField("Activation", 62);
                    EditorGUILayout.PropertyField(activationP, GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                LabelField("Repeat", 62);
                EditorGUILayout.PropertyField(repeatP, GUIContent.none, GUILayout.Width(46));
                GUILayout.Label("times this slot rolls (≥1)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                LabelField("Tier range", 62);
                EditorGUILayout.PropertyField(minTierP, GUIContent.none, GUILayout.Width(58));
                GUILayout.Label("→", GUILayout.Width(14));
                EditorGUILayout.PropertyField(maxTierP, GUIContent.none, GUILayout.Width(58));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(6);

                // ── Shared Pool ───────────────────────────────────────────────
                DrawSharedPoolField(sharedPoolP, poolCfg, isGuar);
            });

            return deleted;
        }

        private void MoveSlot(int from, int to)
        {
            var prop = _selectedSO.FindProperty("_slots");
            prop.MoveArrayElement(from, to);

            if (from < _slotFoldouts.Count && to < _slotFoldouts.Count)
            {
                bool tmp = _slotFoldouts[from];
                _slotFoldouts[from] = _slotFoldouts[to];
                _slotFoldouts[to] = tmp;
            }
        }

        // ── Shared Pool reference + inline preview ───────────────────────────

        private void DrawSharedPoolField(SerializedProperty sharedPoolP, LootPoolConfig poolCfg, bool slotGuar)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Shared Pool", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.PropertyField(sharedPoolP, GUIContent.none, GUILayout.ExpandWidth(true));

            using (new EditorGUI.DisabledScope(poolCfg == null))
            {
                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(40)))
                    EditorGUIUtility.PingObject(poolCfg);
            }

            if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(40)))
            {
                if (poolCfg != null)
                {
                    Selection.activeObject = poolCfg;
                    EditorGUIUtility.PingObject(poolCfg);
                }
                else
                {
                    // Create a new pool asset next to this table and assign it
                    CreateAndAssignPool(sharedPoolP);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (poolCfg == null)
            {
                EditorGUILayout.HelpBox(
                    "No Shared Pool assigned — slot will always skip. Assign a LootPoolConfig or click 'Edit' to create one.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(4);
            DrawPoolPreview(poolCfg, slotGuar);
        }

        private void CreateAndAssignPool(SerializedProperty sharedPoolP)
        {
            string baseFolder = ResolveBaseFolder();
            string poolFolder = baseFolder + "/Pool";
            EnsureFolder(baseFolder, "Pool");

            string name = string.IsNullOrEmpty(_selected.name) ? "Pool" : _selected.name;
            string fileName = $"LootPool_{name}_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}.asset";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{poolFolder}/{fileName}");

            var pool = CreateInstance<LootPoolConfig>();
            AssetDatabase.CreateAsset(pool, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            sharedPoolP.objectReferenceValue = pool;

            EditorGUIUtility.PingObject(pool);
            Selection.activeObject = pool;
        }

        // ── Pool preview (read-only-ish — edits go through the pool's own SerializedObject) ─

        private readonly Dictionary<LootPoolConfig, SerializedObject> _poolSOCache = new();

        private void DrawPoolPreview(LootPoolConfig poolCfg, bool slotGuar)
        {
            if (!_poolSOCache.TryGetValue(poolCfg, out var poolSO) || poolSO == null || poolSO.targetObject == null)
            {
                poolSO = new SerializedObject(poolCfg);
                _poolSOCache[poolCfg] = poolSO;
            }

            poolSO.Update();
            var pool = poolSO.FindProperty("_pool");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pool", EditorStyles.boldLabel, GUILayout.Width(34));
            GUILayout.Label($"{pool.arraySize} entries", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("＋ Entry", EditorStyles.miniButton, GUILayout.Width(60)))
                pool.InsertArrayElementAtIndex(pool.arraySize);
            EditorGUILayout.EndHorizontal();

            if (pool.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Pool is empty — slot will always skip.", MessageType.Warning);
            }
            else
            {
                float totalW = 0f;
                for (int i = 0; i < pool.arraySize; i++)
                    totalW += pool.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").floatValue;

                int del = -1;
                for (int i = 0; i < pool.arraySize; i++)
                    del = DrawPoolEntry(pool.GetArrayElementAtIndex(i), i, totalW, slotGuar, del);
                if (del >= 0) pool.DeleteArrayElementAtIndex(del);
            }

            if (poolSO.ApplyModifiedProperties())
                EditorUtility.SetDirty(poolCfg);
        }

        private int DrawPoolEntry(SerializedProperty e, int idx,
            float totalW, bool slotGuar, int del)
        {
            var itemP = e.FindPropertyRelative("Item");
            var weightP = e.FindPropertyRelative("Weight");
            var minAmt = e.FindPropertyRelative("MinAmount");
            var maxAmt = e.FindPropertyRelative("MaxAmount");
            var minDur = e.FindPropertyRelative("MinDurabilityPercent");
            var maxDur = e.FindPropertyRelative("MaxDurabilityPercent");

            float w = weightP.floatValue;
            float pct = totalW > 0f ? w / totalW * 100f : 0f;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            DrawIcon(itemP, 26f);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(itemP, GUIContent.none, GUILayout.MinWidth(60));
            GUILayout.Label($"{pct:F1}%", EditorStyles.miniLabel, GUILayout.Width(36));
            EditorGUILayout.EndHorizontal();
            DrawWeightBar(pct, slotGuar ? ColGBar : ColWeightBar);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = new Color(1f, 0.38f, 0.38f);
            if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(18), GUILayout.Height(26)))
                del = idx;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Weight", EditorStyles.miniLabel, GUILayout.Width(40));
            EditorGUILayout.PropertyField(weightP, GUIContent.none, GUILayout.Width(48));
            GUILayout.Label("Amount", EditorStyles.miniLabel, GUILayout.Width(45));
            EditorGUILayout.PropertyField(minAmt, GUIContent.none, GUILayout.Width(34));
            GUILayout.Label("–", GUILayout.Width(9));
            EditorGUILayout.PropertyField(maxAmt, GUIContent.none, GUILayout.Width(34));
            GUILayout.Label("Dur%", EditorStyles.miniLabel, GUILayout.Width(27));
            EditorGUILayout.PropertyField(minDur, GUIContent.none, GUILayout.Width(34));
            GUILayout.Label("–", GUILayout.Width(9));
            EditorGUILayout.PropertyField(maxDur, GUIContent.none, GUILayout.Width(34));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);

            return del;
        }

        // ── Bottom bar ────────────────────────────────────────────────────────

        private void DrawBottomBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("＋ New Table", EditorStyles.toolbarButton, GUILayout.Width(90)))
                CreateNewTable();

            if (GUILayout.Button("⟳ Refresh", EditorStyles.toolbarButton, GUILayout.Width(72)))
                Refresh();

            GUILayout.FlexibleSpace();

            if (_selected != null)
            {
                GUILayout.Label($"Editing: {_selected.name}", EditorStyles.miniLabel);
                GUILayout.Space(8);
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Create new table ─────────────────────────────────────────────────

        private bool _showCreateDialog;
        private string _createName = "";

        private void CreateNewTable()
        {
            _createName = "";
            _showCreateDialog = true;
        }

        private void DrawCreateDialog()
        {
            if (!_showCreateDialog) return;

            var fullScreen = new Rect(0, 0, position.width, position.height);
            float dw = 380f, dh = 160f;
            var rc = new Rect(
                (position.width - dw) * 0.5f,
                (position.height - dh) * 0.5f,
                dw, dh);

            var ev = Event.current;
            bool isMouseEvent = ev.type == EventType.MouseDown || ev.type == EventType.MouseUp;

            if (isMouseEvent && fullScreen.Contains(ev.mousePosition))
            {
                if (!rc.Contains(ev.mousePosition))
                    ev.Use();
            }

            EditorGUI.DrawRect(fullScreen, new Color(0f, 0f, 0f, 0.45f));

            GUI.Box(rc, GUIContent.none, EditorStyles.helpBox);

            var bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.83f, 0.83f, 0.83f, 1f);
            EditorGUI.DrawRect(rc, bgColor);

            var inner = new RectOffset(14, 14, 12, 12).Remove(rc);
            GUILayout.BeginArea(inner);

            EditorGUILayout.LabelField("New Loot Table", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            string safeName = string.IsNullOrWhiteSpace(_createName) ? "<name>" : _createName.Trim();
            EditorGUILayout.LabelField("Config:", $"LootTable_{safeName}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("ID:    ", $"ID.loot.table.{safeName}", EditorStyles.miniLabel);
            EditorGUILayout.Space(6);

            GUI.SetNextControlName("CreateNameField");
            _createName = EditorGUILayout.TextField(_createName);

            if (Event.current.type == EventType.Layout)
                EditorGUI.FocusTextInControl("CreateNameField");

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            bool canCreate = !string.IsNullOrWhiteSpace(_createName);

            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button("Create", GUILayout.Height(24)) ||
                    (canCreate && Event.current.type == EventType.KeyDown &&
                     Event.current.keyCode == KeyCode.Return))
                {
                    _showCreateDialog = false;
                    DoCreateTable(_createName.Trim());
                    GUIUtility.ExitGUI();
                }
            }

            if (GUILayout.Button("Cancel", GUILayout.Height(24)) ||
                (Event.current.type == EventType.KeyDown &&
                 Event.current.keyCode == KeyCode.Escape))
            {
                _showCreateDialog = false;
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            Repaint();
        }

        private void DoCreateTable(string name)
        {
            string baseFolder = ResolveBaseFolder();

            string tableFolder = baseFolder + "/Table";
            EnsureFolder(baseFolder, "Table");

            string idsFolder = tableFolder + "/_Ids";
            EnsureFolder(tableFolder, "_Ids");

            string configFileName = $"LootTable_{name}.asset";
            string idFileName = $"ID.loot.table.{name}.asset";

            string configPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{tableFolder}/{configFileName}");
            string idPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{idsFolder}/{idFileName}");

            var tableId = CreateInstance<LootTableId>();

            AssetDatabase.CreateAsset(tableId, idPath);

            var idSO = new SerializedObject(tableId);
            var guidP = idSO.FindProperty("guid");
            if (guidP != null)
            {
                guidP.stringValue = System.Guid.NewGuid().ToString("N");
                idSO.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(tableId);

            var config = CreateInstance<LootTableConfig>();
            AssetDatabase.CreateAsset(config, configPath);

            var cfgSO = new SerializedObject(config);
            var idRef = cfgSO.FindProperty("_id");
            if (idRef != null)
            {
                idRef.objectReferenceValue = tableId;
                cfgSO.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Refresh();
            SelectTable(config);

            EditorGUIUtility.PingObject(config);

            Debug.Log($"[LootTableManager] Created:\n  Config: {configPath}\n  ID:     {idPath}");
        }

        private void DeleteTable(LootTableConfig tbl)
        {
            if (_selected == tbl)
            {
                _selected = null;
                _selectedSO = null;
                _slotFoldouts.Clear();
            }

            string configPath = AssetDatabase.GetAssetPath(tbl);

            var so = new SerializedObject(tbl);
            var idRef = so.FindProperty("_id");
            string idPath = null;

            if (idRef?.objectReferenceValue != null)
                idPath = AssetDatabase.GetAssetPath(idRef.objectReferenceValue);

            if (!string.IsNullOrEmpty(configPath))
                AssetDatabase.DeleteAsset(configPath);

            if (!string.IsNullOrEmpty(idPath))
                AssetDatabase.DeleteAsset(idPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Refresh();
        }

        private string ResolveBaseFolder()
        {
            if (_allTables.Count > 0)
            {
                foreach (var t in _allTables)
                {
                    if (t == null) continue;
                    string p = AssetDatabase.GetAssetPath(t);
                    if (string.IsNullOrEmpty(p)) continue;

                    string dir = System.IO.Path.GetDirectoryName(p)?.Replace('\\', '/');
                    if (dir == null) continue;

                    if (dir.EndsWith("/Table") || dir.EndsWith("\\Table"))
                        return dir.Substring(0, dir.Length - "/Table".Length);

                    return dir;
                }
            }

            const string fallback = "Assets/Resources/Configs/Gameplay/Locations/Loot";
            EnsureFolderPath(fallback);
            return fallback;
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string full = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void EnsureFolderPath(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void DrawWeightBar(float pct, Color color)
        {
            var rc = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.Height(4));
            EditorGUI.DrawRect(rc, new Color(0.15f, 0.15f, 0.15f, 0.35f));
            var fill = new Rect(rc.x, rc.y, rc.width * Mathf.Clamp01(pct / 100f), rc.height);
            EditorGUI.DrawRect(fill, color);
        }

        private void DrawIcon(SerializedProperty itemProp, float size)
        {
            var cfg = itemProp.objectReferenceValue as ItemConfig;
            var rc = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));

            if (cfg != null && cfg.Header.icon != null)
            {
                var spr = cfg.Header.icon;
                var tc = new Rect(
                    spr.rect.x / spr.texture.width,
                    spr.rect.y / spr.texture.height,
                    spr.rect.width / spr.texture.width,
                    spr.rect.height / spr.texture.height);
                GUI.DrawTextureWithTexCoords(rc, spr.texture, tc);
            }
            else
            {
                EditorGUI.DrawRect(rc, new Color(0.12f, 0.12f, 0.12f));
                GUI.Label(rc, "?", EditorStyles.centeredGreyMiniLabel);
            }

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                rc.Contains(Event.current.mousePosition) && cfg != null)
            {
                EditorGUIUtility.PingObject(cfg);
                Event.current.Use();
            }
        }

        private void ColorBox(Color color, System.Action content)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = color + new Color(0, 0, 0, 0.65f);
            EditorGUILayout.BeginVertical(_sectionBoxStyle ?? GUI.skin.box);
            GUI.backgroundColor = prev;
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        private static void LabelField(string text, float width)
            => EditorGUILayout.LabelField(text, GUILayout.Width(width));

        // ── Style init ────────────────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _selectedRowStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { background = MakeTex(1, 1, ColSelected) }
            };

            _normalRowStyle = new GUIStyle(GUI.skin.label);

            _sectionBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 6, 6),
            };

            _slotBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4),
            };

            _tagStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.3f, 1f, 0.5f) },
                fontStyle = FontStyle.Bold,
            };

            _headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
            };
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}