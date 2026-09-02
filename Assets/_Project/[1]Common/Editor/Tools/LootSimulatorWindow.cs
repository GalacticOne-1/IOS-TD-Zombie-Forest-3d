#if UNITY_EDITOR
using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Diagnostics;
using Galactic1.Gameplay;
using UnityEditor;
using UnityEngine;

public class LootSimulatorWindow : EditorWindow
{
    [MenuItem("Tools/Loot Simulator")]
    public static void Open() => GetWindow<LootSimulatorWindow>("Loot Simulator");

    [SerializeField] private LootTableConfig _lootTableConfig;
    [SerializeField] private LootBalanceProfile _balanceProfile;
    [SerializeField] private DepletionCurveConfig _depletionCurve;
    [SerializeField] private Tier _containerTier = Tier.T1;
    [SerializeField] private int _iterations = 1000;
    [SerializeField] private int _openCountStage = 0;
    [SerializeField] private LootSimulationMode _mode = LootSimulationMode.Statistical;

    // Deterministic
    [SerializeField] private int _seed = 12345;

    // Statistical
    [SerializeField] private int _baseSeed = 0;

    private string _report;
    private Vector2 _scroll;
    private bool _configFoldout = true;

    private void OnGUI()
    {
        _configFoldout = EditorGUILayout.Foldout(_configFoldout, "Configuration", true);
        if (_configFoldout)
        {
            EditorGUI.indentLevel++;

            _lootTableConfig = (LootTableConfig)EditorGUILayout.ObjectField(
                "Loot Table", _lootTableConfig, typeof(LootTableConfig), false);
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

        var ready = _lootTableConfig != null && _balanceProfile != null && _depletionCurve != null;

        EditorGUI.BeginDisabledGroup(!ready);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Run Simulation"))
            RunSimulation();

        if (GUILayout.Button("Validate RNG"))
            ValidateRng();

        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        if (!ready)
            EditorGUILayout.HelpBox(
                "Assign Loot Table, Balance Profile and Depletion Curve.", MessageType.Info);

        EditorGUILayout.Space(4);

        if (!string.IsNullOrEmpty(_report))
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    private void RunSimulation()
    {
        var table = BuildTable();
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve);

        var report = _mode == LootSimulationMode.Deterministic
            ? svc.RunDeterministic(table, _containerTier, _seed, _iterations, _openCountStage)
            : svc.RunStatistical(table, _containerTier, _baseSeed, _iterations, _openCountStage);

        _report = LootSimulationService.FormatReport(report);
        Debug.Log(_report);
        Repaint();
    }

    private void ValidateRng()
    {
        var table = BuildTable();
        var svc = new LootSimulationService(_balanceProfile, _depletionCurve);
        _report = svc.ValidateRandomness(table, _containerTier, probeCount: 1000);
        Debug.Log(_report);
        Repaint();
    }

    private LootTableDefinition BuildTable()
        => new LootTableDefinition(
            _lootTableConfig.Id,
            _lootTableConfig.Slots,
            _lootTableConfig.GuaranteedEntries);
}
#endif