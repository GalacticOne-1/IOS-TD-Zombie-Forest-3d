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

    [SerializeField] private LootTableConfig _lootTableConfig;
    [SerializeField] private LocationZonesEditorTool _locationZonesEditorTool;
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

    private string _locationName;
    private int _totalContainers;
    private readonly List<ContainerSection> _containerSections = new();
    private readonly List<ItemAggregate> _locationTotals = new();

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

        // wordWrap выключен: отчёт форматирован фиксированными колонками (padding пробелами),
        // перенос строк поломает выравнивание. Высоту считаем сами через CalcHeight.
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

        EditorGUILayout.Space(6);

        var singleReady = _lootTableConfig != null && _balanceProfile != null && _depletionCurve != null;
        var locationReady = _locationZonesEditorTool != null && _balanceProfile != null && _depletionCurve != null;

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(!singleReady);
        if (GUILayout.Button("Run Simulation")) RunSimulation();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!locationReady);
        if (GUILayout.Button("Simulate Location")) SimulateLocation();
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

        EditorGUILayout.Space(4);

        if (_containerSections.Count > 0)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawTotalsPanel();
            DrawContainerSections();
            EditorGUILayout.EndScrollView();
        }
    }

    // ── actions ──────────────────────────────────────────────────────────────

    private void RunSimulation()
    {
        var table = BuildTable(_lootTableConfig);
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve);

        var report = _mode == LootSimulationMode.Deterministic
            ? svc.RunDeterministic(table, _containerTier, _seed, _iterations, _openCountStage)
            : svc.RunStatistical(table, _containerTier, _baseSeed, _iterations, _openCountStage);

        _locationName = null;
        _locationTotals.Clear();
        _containerSections.Clear();
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
        _containerSections.Add(new ContainerSection("RNG Validation", _containerTier, 0, text));

        Debug.Log(text);
        Repaint();
    }

    private void SimulateLocation()
    {
        var spawnPoints = _locationZonesEditorTool.ZoneRootParent
            .GetComponentsInChildren<LootSpawnPoint>(true)
            .Where(sp => sp.Config != null)
            .ToList();

        _containerSections.Clear();
        _locationTotals.Clear();
        _locationName = _locationZonesEditorTool.name;
        _totalContainers = spawnPoints.Count;

        if (spawnPoints.Count == 0)
        {
            _containerSections.Add(new ContainerSection(
                "No containers found", _containerTier, 0,
                $"No LootSpawnPoint under '{_locationZonesEditorTool.name}' " +
                $"(zone root: '{_locationZonesEditorTool.ZoneRootParent.name}')."));
            Repaint();
            return;
        }

        var groups = spawnPoints
            .GroupBy(sp => sp.Config)
            .OrderBy(g => g.Key.Id.ToString())
            .ToList();

        var svc = new LootSimulationService(_balanceProfile, _depletionCurve);
        var totals = new Dictionary<string, ItemAggregate>();

        foreach (var group in groups)
        {
            var config = group.Key;
            var count = group.Count();

            if (config.LootTableConfig == null)
            {
                _containerSections.Add(new ContainerSection(
                    config.Id.ToString(), config.ContainerTier, count,
                    "SKIPPED: LootTableConfig not assigned.", isSkipped: true));
                continue;
            }

            var table = BuildTable(config.LootTableConfig);
            var report = _mode == LootSimulationMode.Deterministic
                ? svc.RunDeterministic(table, config.ContainerTier, _seed, _iterations, _openCountStage)
                : svc.RunStatistical(table, config.ContainerTier, _baseSeed, _iterations, _openCountStage);

            _containerSections.Add(new ContainerSection(
                $"{config.Id}  ({config.LootTableConfig.Id})", config.ContainerTier, count,
                LootSimulationService.FormatReport(report)));

            AccumulateTotals(totals, report, count);
        }

        _locationTotals.AddRange(totals.Values.OrderByDescending(a => a.ExpectedAmount));
        Repaint();
    }

    private static void AccumulateTotals(Dictionary<string, ItemAggregate> totals,
        LootSimulationService.SimulationReport report, int count)
    {
        if (report.Iterations <= 0) return;

        foreach (var kv in report.ItemTotalAmount)
        {
            var avgAmountPerOpen = kv.Value / (float)report.Iterations;
            GetOrAdd(totals, kv.Key).ExpectedAmount += avgAmountPerOpen * count;
        }

        foreach (var kv in report.ItemFrequency)
        {
            var avgAppearancePerOpen = kv.Value / (float)report.Iterations;
            GetOrAdd(totals, kv.Key).ExpectedAppearances += avgAppearancePerOpen * count;
        }
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

    // ── drawing ──────────────────────────────────────────────────────────────

    private void DrawTotalsPanel()
    {
        if (_locationTotals.Count == 0) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"◆ Location Totals — {_locationName}", _totalsHeaderStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Copy List", GUILayout.Width(80)))
        {
            EditorGUIUtility.systemCopyBuffer = BuildTotalsText();
            ShowNotification(new GUIContent("Copied to clipboard"));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(
            $"Expected resources across {_totalContainers} containers (per full sweep, sorted by amount):",
            _totalsSubStyle);
        EditorGUILayout.Space(4);

        var maxAmount = Mathf.Max(0.0001f, _locationTotals[0].ExpectedAmount);

        foreach (var item in _locationTotals)
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

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }

    /// <summary>
    /// Плоский текстовый список ресурсов локации, готовый для вставки в таблицу/Excel/чат —
    /// одна строка на предмет, значения через табуляцию.
    /// </summary>
    private string BuildTotalsText()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Location Totals — {_locationName} ({_totalContainers} containers)");
        sb.AppendLine("Item\tTotal Amount\tExpected Drops");

        foreach (var item in _locationTotals)
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