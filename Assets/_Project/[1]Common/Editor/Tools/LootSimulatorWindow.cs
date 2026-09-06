#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Diagnostics;
using Galactic1.RaidLoot.Scene;
using Galactic1.Gameplay;
using UnityEditor;
using UnityEngine;

public class LootSimulatorWindow : EditorWindow
{
    [MenuItem("Tools/Loot Simulator")]
    public static void Open() => GetWindow<LootSimulatorWindow>("Loot Simulator");

    [Header("Location")] [SerializeField] private LocationZonesEditorTool _locationZonesEditorTool;
    [SerializeField] private LocationLootProfileConfig _lootProfileConfig;

    [Header("Batch (multiple locations)")] [SerializeField]
    private List<LocationBatchEntry> _batchEntries = new();

    [Header("Container")] [SerializeField] private LootTableConfig _lootTableConfig;
    [SerializeField] private LootBalanceProfile _balanceProfile;
    [SerializeField] private DepletionCurveConfig _depletionCurve;
    [SerializeField] private Tier _containerTier = Tier.T1;
    [SerializeField] private int _iterations = 1000;
    [SerializeField] private int _openCountStage = 0;
    [SerializeField] private LootSimulationMode _mode = LootSimulationMode.Statistical;

    [SerializeField] private int _seed = 12345;
    [SerializeField] private int _baseSeed = 0;

    private Vector2 _scroll;
    private bool _configFoldout = true;
    private bool _batchFoldout = true;

    private string _locationName;
    private int _totalContainers;
    private readonly List<ContainerSection> _containerSections = new();
    private readonly List<ItemAggregate> _locationTotals = new();

    // Результаты batch-симуляции (Simulate All Locations) — отдельно от single-location view.
    private readonly List<LocationSimResult> _batchResults = new();

    // ── styling ──────────────────────────────────────────────────────────────

    private static readonly Color HeaderColor = new(0.4f, 0.78f, 1f);
    private static readonly Color TotalsHeaderColor = new(0.55f, 0.95f, 0.6f);
    private static readonly Color TotalsBarColor = new(0.35f, 0.85f, 0.55f, 0.55f);
    private static readonly Color SkippedColor = new(1f, 0.45f, 0.45f);
    private static readonly Color SeparatorColor = new(1f, 1f, 1f, 0.12f);

    private GUIStyle _headerStyle;
    private GUIStyle _totalsHeaderStyle;
    private GUIStyle _totalsSubStyle;
    private GUIStyle _containerNameStyle;
    private GUIStyle _skippedNameStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _barLabelStyle;

    private void EnsureStyles()
    {
        if (_headerStyle != null) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, normal = { textColor = HeaderColor } };
        _totalsHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 13, normal = { textColor = TotalsHeaderColor } };
        _totalsSubStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic };
        _containerNameStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
        _skippedNameStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 12, normal = { textColor = SkippedColor } };

        _bodyStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = false,
            richText = false,
            clipping = TextClipping.Clip
        };

        _barLabelStyle = new GUIStyle(EditorStyles.label)
            { alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
    }

    private void OnGUI()
    {
        EnsureStyles();

        _configFoldout = EditorGUILayout.Foldout(_configFoldout, "Configuration", true);
        if (_configFoldout)
        {
            EditorGUI.indentLevel++;

            _lootTableConfig = (LootTableConfig)EditorGUILayout.ObjectField(
                "Loot Table", _lootTableConfig, typeof(LootTableConfig), false);
            _locationZonesEditorTool = (LocationZonesEditorTool)EditorGUILayout.ObjectField(
                "Location", _locationZonesEditorTool, typeof(LocationZonesEditorTool), true);
            _lootProfileConfig = (LocationLootProfileConfig)EditorGUILayout.ObjectField(
                "Location Loot Profile", _lootProfileConfig, typeof(LocationLootProfileConfig), false);
            _balanceProfile = (LootBalanceProfile)EditorGUILayout.ObjectField(
                "Balance Profile", _balanceProfile, typeof(LootBalanceProfile), false);
            _depletionCurve = (DepletionCurveConfig)EditorGUILayout.ObjectField(
                "Depletion Curve", _depletionCurve, typeof(DepletionCurveConfig), false);

            _containerTier = (Tier)EditorGUILayout.EnumPopup("Container Tier", _containerTier);
            _iterations = EditorGUILayout.IntField("Iterations", _iterations);
            _openCountStage = EditorGUILayout.IntSlider("Open Count (stage)", _openCountStage, 0, 3);
            _mode = (LootSimulationMode)EditorGUILayout.EnumPopup("Mode", _mode);

            if (_mode == LootSimulationMode.Deterministic)
                _seed = EditorGUILayout.IntField("Seed", _seed);
            else
                _baseSeed = EditorGUILayout.IntField("Base Seed", _baseSeed);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        _batchFoldout = EditorGUILayout.Foldout(_batchFoldout, "Batch Locations", true);
        if (_batchFoldout)
        {
            EditorGUI.indentLevel++;
            DrawBatchEntryList();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        var singleReady = _lootTableConfig != null && _balanceProfile != null && _depletionCurve != null;
        var locationReady = _locationZonesEditorTool != null && _balanceProfile != null && _depletionCurve != null;
        var batchReady = _balanceProfile != null && _depletionCurve != null &&
                         _batchEntries.Count > 0 && _batchEntries.All(e => e.Location != null);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(!singleReady);
        if (GUILayout.Button("Run Simulation")) RunSimulation();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!locationReady);
        if (GUILayout.Button("Simulate Location")) SimulateLocation();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!batchReady);
        if (GUILayout.Button("Simulate All Locations")) SimulateAllLocations();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!singleReady);
        if (GUILayout.Button("Validate RNG")) ValidateRng();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (!singleReady)
            EditorGUILayout.HelpBox("Assign Loot Table, Balance Profile and Depletion Curve.", MessageType.Info);
        if (!locationReady)
            EditorGUILayout.HelpBox(
                "Assign Location, Balance Profile and Depletion Curve to simulate a whole location.", MessageType.Info);
        if (!batchReady)
            EditorGUILayout.HelpBox(
                "Add at least one Batch Location (with an assigned Location) plus Balance Profile and Depletion Curve.",
                MessageType.Info);

        EditorGUILayout.Space(4);

        if (_batchResults.Count > 0)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawBatchResults();
            EditorGUILayout.EndScrollView();
        }
        else if (_containerSections.Count > 0)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawTotalsPanel();
            DrawContainerSections();
            EditorGUILayout.EndScrollView();
        }
    }

    // ── batch entry list UI ──────────────────────────────────────────────────

    private void DrawBatchEntryList()
    {
        for (int i = 0; i < _batchEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            var entry = _batchEntries[i];
            entry.Location = (LocationZonesEditorTool)EditorGUILayout.ObjectField(
                entry.Location, typeof(LocationZonesEditorTool), true);
            entry.LootProfile = (LocationLootProfileConfig)EditorGUILayout.ObjectField(
                entry.LootProfile, typeof(LocationLootProfileConfig), false);
            _batchEntries[i] = entry;

            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                _batchEntries.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Location", GUILayout.Width(140)))
            _batchEntries.Add(new LocationBatchEntry());
    }

    // ── actions ──────────────────────────────────────────────────────────────

    private void RunSimulation()
    {
        var table = BuildTable(_lootTableConfig);
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve, BuildLootProfile(_lootProfileConfig));

        var report = _mode == LootSimulationMode.Deterministic
            ? svc.RunDeterministic(table, _containerTier, _seed, _iterations, _openCountStage)
            : svc.RunStatistical(table, _containerTier, _baseSeed, _iterations, _openCountStage);

        _locationName = null;
        _locationTotals.Clear();
        _containerSections.Clear();
        _batchResults.Clear();
        _containerSections.Add(new ContainerSection(
            _lootTableConfig.name, _containerTier, 0, LootSimulationService.FormatReport(report)));

        Debug.Log(LootSimulationService.FormatReport(report));
        Repaint();
    }

    private void ValidateRng()
    {
        var table = BuildTable(_lootTableConfig);
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve);
        var text = svc.ValidateRandomness(table, _containerTier, probeCount: 1000);

        _locationName = null;
        _locationTotals.Clear();
        _containerSections.Clear();
        _batchResults.Clear();
        _containerSections.Add(new ContainerSection("RNG Validation", _containerTier, 0, text));

        Debug.Log(text);
        Repaint();
    }

    private void SimulateLocation()
    {
        var result = SimulateOneLocation(_locationZonesEditorTool, _lootProfileConfig);

        _batchResults.Clear();
        _containerSections.Clear();
        _locationTotals.Clear();

        _locationName = result.LocationName;
        _totalContainers = result.TotalContainers;
        _containerSections.AddRange(result.Sections);
        _locationTotals.AddRange(result.Totals);

        Repaint();
    }

    private void SimulateAllLocations()
    {
        _containerSections.Clear();
        _locationTotals.Clear();
        _locationName = null;
        _batchResults.Clear();

        foreach (var entry in _batchEntries)
        {
            if (entry.Location == null) continue;
            _batchResults.Add(SimulateOneLocation(entry.Location, entry.LootProfile));
        }

        Repaint();
    }

    /// <summary>
    /// Прогоняет симуляцию для одной локации: находит все LootSpawnPoint,
    /// группирует по конфигу контейнера, симулирует каждую группу и агрегирует totals.
    /// Используется и для одиночного "Simulate Location", и для batch-режима —
    /// поведение идентично в обоих случаях.
    /// </summary>
    private LocationSimResult SimulateOneLocation(
        LocationZonesEditorTool locationTool,
        LocationLootProfileConfig profileConfig)
    {
        var result = new LocationSimResult { LocationName = locationTool.name };

        var spawnPoints = locationTool.ZoneRootParent
            .GetComponentsInChildren<LootSpawnPoint>(true)
            .Where(sp => sp.Config != null)
            .ToList();

        result.TotalContainers = spawnPoints.Count;

        if (spawnPoints.Count == 0)
        {
            result.Sections.Add(new ContainerSection(
                "No containers found", _containerTier, 0,
                $"No LootSpawnPoint under '{locationTool.name}' " +
                $"(zone root: '{locationTool.ZoneRootParent.name}')."));
            return result;
        }

        var groups = spawnPoints
            .GroupBy(sp => sp.Config)
            .OrderBy(g => g.Key.Id.ToString())
            .ToList();

        var lootProfile = BuildLootProfile(profileConfig);
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve, lootProfile);
        var totals = new Dictionary<string, ItemAggregate>();

        foreach (var group in groups)
        {
            var config = group.Key;
            var count = group.Count();

            if (config.LootTableConfig == null)
            {
                result.Sections.Add(new ContainerSection(
                    config.Id.ToString(), config.ContainerTier, count,
                    "SKIPPED: LootTableConfig not assigned.", isSkipped: true));
                continue;
            }

            var table = BuildTable(config.LootTableConfig);
            var report = _mode == LootSimulationMode.Deterministic
                ? svc.RunDeterministic(table, config.ContainerTier, _seed, _iterations, _openCountStage)
                : svc.RunStatistical(table, config.ContainerTier, _baseSeed, _iterations, _openCountStage);

            result.Sections.Add(new ContainerSection(
                $"{config.Id}  ({config.LootTableConfig.Id})", config.ContainerTier, count,
                LootSimulationService.FormatReport(report)));

            AccumulateTotals(totals, report, count);
        }

        result.Totals.AddRange(totals.Values.OrderByDescending(a => a.ExpectedAmount));
        return result;
    }

    private LocationLootProfile BuildLootProfile(LocationLootProfileConfig config)
    {
        if (config == null) return null;

        return new LocationLootProfile(
            default, // LocationId — для симулятора не важен, используется только для queries по item/category
            config.CategoryMultipliers,
            config.ItemMultipliers);
    }

    private static void AccumulateTotals(Dictionary<string, ItemAggregate> totals,
        LootSimulationService.SimulationReport report, int count)
    {
        if (report.Iterations <= 0) return;

        foreach (var kv in report.ItemTotalAmount)
            GetOrAdd(totals, kv.Key).ExpectedAmount += kv.Value * count;

        foreach (var kv in report.ItemFrequency)
            GetOrAdd(totals, kv.Key).ExpectedAppearances += kv.Value * count;
    }

    private static ItemAggregate GetOrAdd(Dictionary<string, ItemAggregate> totals, string name)
    {
        if (!totals.TryGetValue(name, out var agg))
        {
            agg = new ItemAggregate { Name = name };
            totals[name] = agg;
        }

        return agg;
    }

    private LootTableDefinition BuildTable(LootTableConfig config)
        => new LootTableDefinition(config.Id, config.Slots, config.GuaranteedEntries);

    // ── drawing: single-location view ────────────────────────────────────────

    private void DrawTotalsPanel()
    {
        if (_locationTotals.Count == 0) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"◆ Location Totals — {_locationName}", _totalsHeaderStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Copy List", GUILayout.Width(80)))
        {
            EditorGUIUtility.systemCopyBuffer = BuildTotalsText(_locationName, _totalContainers, _locationTotals);
            ShowNotification(new GUIContent("Copied to clipboard"));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(
            $"Expected resources across {_totalContainers} containers over {_iterations} visits:",
            _totalsSubStyle);
        EditorGUILayout.Space(4);

        DrawTotalsBars(_locationTotals);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }

    private void DrawTotalsBars(List<ItemAggregate> totals)
    {
        var maxAmount = Mathf.Max(0.0001f, totals[0].ExpectedAmount);

        foreach (var item in totals)
        {
            var barRect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
            EditorGUI.DrawRect(barRect, new Color(1f, 1f, 1f, 0.05f));

            var fillWidth = barRect.width * Mathf.Clamp01(item.ExpectedAmount / maxAmount);
            EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, fillWidth, barRect.height), TotalsBarColor);

            EditorGUI.LabelField(
                barRect,
                $"  {item.Name}   —   {item.ExpectedAmount:F1} total   (~{item.ExpectedAppearances:F1} drops)",
                _barLabelStyle);
        }
    }

    /// <summary>
    /// Плоский текстовый список ресурсов локации, готовый для вставки в таблицу/Excel/чат —
    /// одна строка на предмет, значения через табуляцию.
    /// </summary>
    private static string BuildTotalsText(string locationName, int totalContainers, List<ItemAggregate> totals)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Location Totals — {locationName} ({totalContainers} containers)");
        sb.AppendLine("Item\tTotal Amount\tExpected Drops");

        foreach (var item in totals)
            sb.AppendLine($"{item.Name}\t{item.ExpectedAmount:F1}\t{item.ExpectedAppearances:F1}");

        return sb.ToString();
    }

    private void DrawContainerSections()
    {
        if (!string.IsNullOrEmpty(_locationName))
        {
            EditorGUILayout.LabelField($"=== Location Simulation: {_locationName} ===", _headerStyle);
            EditorGUILayout.LabelField(
                $"Total containers: {_totalContainers} | Unique configs: {_containerSections.Count}");
            EditorGUILayout.Space(6);
        }

        foreach (var section in _containerSections)
        {
            var headerRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            var bg = section.IsSkipped ? SkippedColor : GetTierColor(section.Tier);
            bg.a = 0.18f;
            EditorGUI.DrawRect(headerRect, bg);

            var titleText = section.Count > 0 ? $"{section.Title}   ×{section.Count}" : section.Title;
            EditorGUI.LabelField(headerRect, "  " + titleText,
                section.IsSkipped ? _skippedNameStyle : _containerNameStyle);

            DrawMultilineText(section.Body, _bodyStyle);

            var sepRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(sepRect, SeparatorColor);
            EditorGUILayout.Space(4);
        }
    }

    // ── drawing: batch (multiple locations) view ─────────────────────────────

    private void DrawBatchResults()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"◆ Batch Simulation — {_batchResults.Count} location(s)", _totalsHeaderStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Copy List", GUILayout.Width(80)))
        {
            EditorGUIUtility.systemCopyBuffer = BuildBatchTotalsText();
            ShowNotification(new GUIContent("Copied to clipboard"));
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Expected resources per location, over {_iterations} visits each:",
            _totalsSubStyle);
        EditorGUILayout.Space(6);

        foreach (var result in _batchResults)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField(
                $"{result.LocationName}   —   {result.TotalContainers} containers", _headerStyle);
            EditorGUILayout.Space(2);

            if (result.Totals.Count == 0)
                EditorGUILayout.LabelField("No loot data.", _totalsSubStyle);
            else
                DrawTotalsBars(result.Totals);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }
    }

    /// <summary>
    /// Объединённый текстовый отчёт по всем локациям batch-режима —
    /// один блок на локацию, разделены пустой строкой. Готов для вставки в Excel/чат.
    /// </summary>
    private string BuildBatchTotalsText()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Batch Simulation — {_batchResults.Count} location(s), {_iterations} visits each");
        sb.AppendLine();

        foreach (var result in _batchResults)
        {
            sb.AppendLine($"=== {result.LocationName} ({result.TotalContainers} containers) ===");
            sb.AppendLine("Item\tTotal Amount\tExpected Drops");

            foreach (var item in result.Totals)
                sb.AppendLine($"{item.Name}\t{item.ExpectedAmount:F1}\t{item.ExpectedAppearances:F1}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// LabelField резервирует высоту только под одну строку — многострочный отчёт
    /// либо обрезается, либо съедает соседние элементы. Тут высота считается явно
    /// через CalcHeight под фактический текст, плюс SelectableLabel — чтобы отчёт
    /// можно было выделить/скопировать мышкой.
    /// </summary>
    private static void DrawMultilineText(string text, GUIStyle style)
    {
        if (string.IsNullOrEmpty(text)) return;

        var width = EditorGUIUtility.currentViewWidth - 24f;
        var content = new GUIContent(text);
        var height = style.CalcHeight(content, width);

        var rect = EditorGUILayout.GetControlRect(false, height);
        EditorGUI.SelectableLabel(rect, text, style);
    }

    /// <summary>Стабильный, но детерминированный цвет по названию тира — не завязан на конкретные значения enum.</summary>
    private static Color GetTierColor(Tier tier)
    {
        var hash = tier.ToString().GetHashCode();
        var hue = (Mathf.Abs(hash) % 360) / 360f;
        return Color.HSVToRGB(hue, 0.55f, 0.95f);
    }

    [System.Serializable]
    private struct LocationBatchEntry
    {
        public LocationZonesEditorTool Location;
        public LocationLootProfileConfig LootProfile;
    }

    private sealed class LocationSimResult
    {
        public string LocationName;
        public int TotalContainers;
        public readonly List<ItemAggregate> Totals = new();
        public readonly List<ContainerSection> Sections = new();
    }

    private sealed class ItemAggregate
    {
        public string Name;
        public float ExpectedAmount;
        public float ExpectedAppearances;
    }

    private readonly struct ContainerSection
    {
        public readonly string Title;
        public readonly Tier Tier;
        public readonly int Count;
        public readonly string Body;
        public readonly bool IsSkipped;

        public ContainerSection(string title, Tier tier, int count, string body, bool isSkipped = false)
        {
            Title = title;
            Tier = tier;
            Count = count;
            Body = body;
            IsSkipped = isSkipped;
        }
    }
}
#endif