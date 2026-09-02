#if UNITY_EDITOR
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Combat
{
    /// <summary>
    /// Tools → Combat → Weapon Accuracy Simulator
    ///
    /// Одиночное оружие: HitRate по дистанциям + график.
    /// Дробовик:         среднее попавших дробин + распределение per-shot.
    ///
    /// Тип симуляции определяется автоматически по WeaponDefinition.projectilesPerShot.
    /// </summary>
    public sealed class WeaponAccuracySimulatorWindow : EditorWindow
    {
        // ── Настройки ─────────────────────────────────────────────────────

        private WeaponDefinition _weaponDef;
        private float _distance = 20f;
        private int _shotCount = 10000;
        private float _targetRadius = WeaponAccuracySimulator.DefaultTargetRadius;

        // Серия
        private bool _showSeries = false;
        private string _seriesInput = "5, 10, 15, 20, 30, 40, 50";

        // Дробовик — фиксированные дистанции по умолчанию
        private string _shotgunSeriesInput = "3, 5, 10, 15";

        // Результаты — обычное оружие
        private SimResult? _singleResult;
        private List<SimResult> _seriesResults;

        // Результаты — дробовик
        private WeaponAccuracySimulator.ShotgunSimulationResult _shotgunSingle;
        private List<WeaponAccuracySimulator.ShotgunSimulationResult> _shotgunSeries;

        // Выбранная дистанция для показа распределения в серии
        private int _selectedShotgunIndex = 0;

        private string _errorMessage;
        private bool _showChart = true;
        private Vector2 _scroll;

        // Отслеживаем смену SO
        private WeaponDefinition _lastWeaponDef;

        // ── Menu ──────────────────────────────────────────────────────────

        [MenuItem("Tools/Test Combat/Weapon Accuracy Simulator")]
        public static void Open()
        {
            var w = GetWindow<WeaponAccuracySimulatorWindow>("Accuracy Simulator");
            w.minSize = new Vector2(500f, 640f);
            w.Show();
        }

        // ── GUI ───────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (_weaponDef != _lastWeaponDef)
            {
                _singleResult = null;
                _seriesResults = null;
                _shotgunSingle = null;
                _shotgunSeries = null;
                _errorMessage = null;
                _selectedShotgunIndex = 0;
                _lastWeaponDef = _weaponDef;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawHeader();
            GUILayout.Space(8f);
            DrawConfig();
            GUILayout.Space(8f);

            bool isShotgun = IsShotgun();

            if (isShotgun)
                DrawShotgunSeriesConfig();
            else
                DrawSeriesConfig();

            GUILayout.Space(12f);
            DrawRunButtons(isShotgun);
            GUILayout.Space(12f);

            if (!string.IsNullOrEmpty(_errorMessage))
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);

            // ── Результаты ──
            if (isShotgun)
            {
                if (_shotgunSingle != null && !_showSeries)
                    DrawShotgunResult(_shotgunSingle);

                if (_shotgunSeries != null && _shotgunSeries.Count > 0)
                    DrawShotgunSeriesResults();
            }
            else
            {
                if (_singleResult.HasValue && !_showSeries)
                    DrawSingleResult(_singleResult.Value);

                if (_seriesResults != null && _seriesResults.Count > 0)
                {
                    DrawSeriesTable();
                    GUILayout.Space(8f);
                    if (_showChart)
                        DrawChart();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        // ── Config ────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            var s = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Weapon Accuracy Simulator", s);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawConfig()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _weaponDef = (WeaponDefinition)EditorGUILayout.ObjectField(
                    "Weapon Definition", _weaponDef, typeof(WeaponDefinition), false);

                if (!_showSeries)
                    _distance = EditorGUILayout.FloatField("Distance (m)", _distance);

                _shotCount = EditorGUILayout.IntField("Shots Count", _shotCount);
                _targetRadius = EditorGUILayout.FloatField("Target Radius (m)", _targetRadius);

                if (_weaponDef != null)
                {
                    var d = _weaponDef.ToData();
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Weapon Stats", EditorStyles.miniBoldLabel);
                        string pelletInfo = d.ProjectilesPerShot > 1
                            ? $"  |  Pellets: {d.ProjectilesPerShot}"
                            : "";
                        EditorGUILayout.LabelField(
                            $"BaseSpread: {d.BaseSpreadDeg}°  |  " +
                            $"EffRange: {d.EffectiveRange}m  |  " +
                            $"MaxRange: {d.MaxRange}m  |  " +
                            $"Penalty: ×{d.MaxRangeSpreadPenalty}" + pelletInfo,
                            EditorStyles.miniLabel);
                    }
                }
            }
        }

        private void DrawSeriesConfig()
        {
            _showSeries = EditorGUILayout.ToggleLeft("Run Distance Series", _showSeries,
                EditorStyles.boldLabel);
            if (!_showSeries) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Distances (comma-separated, meters):", EditorStyles.miniLabel);
                _seriesInput = EditorGUILayout.TextField(_seriesInput);
                _showChart = EditorGUILayout.ToggleLeft("Show Chart", _showChart);
            }
        }

        private void DrawShotgunSeriesConfig()
        {
            _showSeries = EditorGUILayout.ToggleLeft("Run Distance Series  (shotgun mode)",
                _showSeries, EditorStyles.boldLabel);
            if (!_showSeries) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Distances (comma-separated, meters):", EditorStyles.miniLabel);
                _shotgunSeriesInput = EditorGUILayout.TextField(_shotgunSeriesInput);
            }
        }

        private void DrawRunButtons(bool isShotgun)
        {
            using (new EditorGUI.DisabledScope(_weaponDef == null))
            {
                var btn = new GUIStyle(GUI.skin.button) { fixedHeight = 32f };
                string label = _showSeries ? "▶  Run Series" : "▶  Run Simulation";
                if (isShotgun) label += "  (Shotgun)";

                if (GUILayout.Button(label, btn))
                {
                    if (isShotgun) RunShotgun();
                    else if (_showSeries) RunSeries();
                    else RunSingle();
                }
            }

            if (_weaponDef == null)
                EditorGUILayout.HelpBox("Select a WeaponDefinition to run simulation.", MessageType.Info);
        }

        // ── Single results ────────────────────────────────────────────────

        private void DrawSingleResult(SimResult r)
        {
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Weapon:    {_weaponDef.name}");
                EditorGUILayout.LabelField($"Distance:  {r.Distance:F1} m");
                EditorGUILayout.LabelField($"Spread:    {r.SpreadDeg:F2}°");
                EditorGUILayout.LabelField($"Shots:     {r.ShotCount:N0}");
                EditorGUILayout.LabelField($"Hits:      {r.Hits:N0}");
                EditorGUILayout.LabelField($"Misses:    {r.Misses:N0}");

                var rs = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 16, normal = { textColor = HitRateColor(r.HitRate) } };
                EditorGUILayout.LabelField($"Hit Rate:  {r.HitRate * 100f:F2}%", rs);
            }
        }

        // ── Shotgun results ───────────────────────────────────────────────

        private void DrawShotgunResult(WeaponAccuracySimulator.ShotgunSimulationResult r)
        {
            EditorGUILayout.LabelField("Shotgun Result", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Weapon:             {_weaponDef.name}");
                EditorGUILayout.LabelField($"Distance:           {r.Distance:F1} m");
                EditorGUILayout.LabelField($"Pellets per shot:   {r.PelletsPerShot}");
                EditorGUILayout.LabelField($"Shots simulated:    {r.ShotCount:N0}");

                GUILayout.Space(4f);
                var avgStyle = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 13, normal = { textColor = HitRateColor(r.PelletHitRate) } };
                EditorGUILayout.LabelField(
                    $"Avg pellets hit:    {r.AvgPelletsHit:F1} / {r.PelletsPerShot}  " +
                    $"({r.PelletHitRate * 100f:F1}%)", avgStyle);
                EditorGUILayout.LabelField($"Avg pellets miss:   {r.AvgPelletsMiss:F1}");

                GUILayout.Space(8f);
                DrawPelletDistribution(r);
            }
        }

        private void DrawShotgunSeriesResults()
        {
            EditorGUILayout.LabelField($"Shotgun Series — {_weaponDef.name}", EditorStyles.boldLabel);

            // Сводная таблица
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Distance", EditorStyles.boldLabel, GUILayout.Width(80f));
                    EditorGUILayout.LabelField("Avg Hit", EditorStyles.boldLabel, GUILayout.Width(80f));
                    EditorGUILayout.LabelField("/ Total", EditorStyles.boldLabel, GUILayout.Width(60f));
                    EditorGUILayout.LabelField("Avg Miss", EditorStyles.boldLabel, GUILayout.Width(80f));
                    EditorGUILayout.LabelField("Pellet %", EditorStyles.boldLabel, GUILayout.Width(70f));
                    EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true));
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                for (int i = 0; i < _shotgunSeries.Count; i++)
                {
                    var r = _shotgunSeries[i];
                    var rateStyle = new GUIStyle(EditorStyles.label)
                        { normal = { textColor = HitRateColor(r.PelletHitRate) } };

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{r.Distance:F0} m", GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{r.AvgPelletsHit:F1}", rateStyle, GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"/ {r.PelletsPerShot}", GUILayout.Width(60f));
                        EditorGUILayout.LabelField($"{r.AvgPelletsMiss:F1}", GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{r.PelletHitRate * 100f:F1}%", rateStyle, GUILayout.Width(70f));

                        var barRect = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
                        DrawBar(barRect, r.PelletHitRate);
                    }
                }
            }

            GUILayout.Space(10f);

            // Детальное распределение с табами по дистанциям
            EditorGUILayout.LabelField("Pellet Distribution", EditorStyles.boldLabel);

            // Таб-переключатель
            var tabLabels = new string[_shotgunSeries.Count];
            for (int i = 0; i < _shotgunSeries.Count; i++)
                tabLabels[i] = $"{_shotgunSeries[i].Distance:F0}m";

            _selectedShotgunIndex = GUILayout.Toolbar(
                Mathf.Clamp(_selectedShotgunIndex, 0, _shotgunSeries.Count - 1),
                tabLabels);

            if (_selectedShotgunIndex < _shotgunSeries.Count)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    DrawPelletDistribution(_shotgunSeries[_selectedShotgunIndex]);
            }
        }

        /// <summary>
        /// Рисует таблицу распределения для одного результата дробовика.
        /// Например: 0 pellets 0%, 1 pellet 1%, ..., 8 pellets 4%
        /// </summary>
        private static void DrawPelletDistribution(
            WeaponAccuracySimulator.ShotgunSimulationResult r)
        {
            EditorGUILayout.LabelField(
                $"Distribution at {r.Distance:F0}m  (per shot, n={r.ShotCount:N0})",
                EditorStyles.miniBoldLabel);

            GUILayout.Space(2f);

            for (int k = 0; k <= r.PelletsPerShot; k++)
            {
                float frac = r.DistributionFraction(k);
                int count = r.PelletDistribution[k];
                if (count == 0) continue;

                string pelletLabel = k == 1 ? "1 pellet " : $"{k} pellets";

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(pelletLabel, GUILayout.Width(72f));
                    EditorGUILayout.LabelField($"{frac * 100f:F1}%",
                        new GUIStyle(EditorStyles.label)
                        {
                            normal = { textColor = HitRateColor((float)k / r.PelletsPerShot) }
                        },
                        GUILayout.Width(50f));

                    // Мини-бар
                    var barRect = GUILayoutUtility.GetRect(0f, 14f, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
                    EditorGUI.DrawRect(
                        new Rect(barRect.x, barRect.y, barRect.width * frac, barRect.height),
                        HitRateColor((float)k / r.PelletsPerShot));

                    EditorGUILayout.LabelField($"({count:N0})",
                        EditorStyles.miniLabel, GUILayout.Width(60f));
                }
            }
        }

        // ── Series results (обычное оружие) ───────────────────────────────

        private void DrawSeriesTable()
        {
            EditorGUILayout.LabelField($"Results — {_weaponDef.name}", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Distance", EditorStyles.boldLabel, GUILayout.Width(80f));
                    EditorGUILayout.LabelField("Spread", EditorStyles.boldLabel, GUILayout.Width(70f));
                    EditorGUILayout.LabelField("Hits", EditorStyles.boldLabel, GUILayout.Width(70f));
                    EditorGUILayout.LabelField("Misses", EditorStyles.boldLabel, GUILayout.Width(70f));
                    EditorGUILayout.LabelField("Hit Rate", EditorStyles.boldLabel, GUILayout.Width(80f));
                    EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true));
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (var r in _seriesResults)
                {
                    var rs = new GUIStyle(EditorStyles.label)
                        { normal = { textColor = HitRateColor(r.HitRate) } };
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{r.Distance:F0} m", GUILayout.Width(80f));
                        EditorGUILayout.LabelField($"{r.SpreadDeg:F2}°", GUILayout.Width(70f));
                        EditorGUILayout.LabelField($"{r.Hits:N0}", GUILayout.Width(70f));
                        EditorGUILayout.LabelField($"{r.Misses:N0}", GUILayout.Width(70f));
                        EditorGUILayout.LabelField($"{r.HitRate * 100f:F1}%", rs, GUILayout.Width(80f));
                        var barRect = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
                        DrawBar(barRect, r.HitRate);
                    }
                }
            }
        }

        // ── Chart ─────────────────────────────────────────────────────────

        private void DrawChart()
        {
            EditorGUILayout.LabelField("Hit Rate by Distance", EditorStyles.boldLabel);

            var chartRect = GUILayoutUtility.GetRect(
                GUILayoutUtility.GetLastRect().width, 200f, GUILayout.ExpandWidth(true));

            if (Event.current.type != EventType.Repaint) return;

            EditorGUI.DrawRect(chartRect, new Color(0.15f, 0.15f, 0.15f));
            if (_seriesResults == null || _seriesResults.Count < 2) return;

            float minDist = _seriesResults[0].Distance;
            float maxDist = _seriesResults[_seriesResults.Count - 1].Distance;
            float distRange = Mathf.Max(maxDist - minDist, 1f);

            var gridColor = new Color(0.3f, 0.3f, 0.3f);
            for (int g = 0; g <= 4; g++)
            {
                float py = chartRect.yMax - (g / 4f) * chartRect.height;
                EditorGUI.DrawRect(new Rect(chartRect.xMin, py - 0.5f, chartRect.width, 1f), gridColor);
            }

            if (_weaponDef != null)
            {
                var def = _weaponDef.ToData();
                DrawVerticalLine(chartRect, def.EffectiveRange, minDist, distRange,
                    new Color(0.2f, 0.8f, 0.2f, 0.5f), "Eff");
                DrawVerticalLine(chartRect, def.MaxRange, minDist, distRange,
                    new Color(0.8f, 0.3f, 0.2f, 0.5f), "Max");
            }

            var lineColor = new Color(0.3f, 0.8f, 1f);
            for (int i = 0; i < _seriesResults.Count - 1; i++)
            {
                var a = _seriesResults[i];
                var b = _seriesResults[i + 1];
                float x1 = chartRect.xMin + (a.Distance - minDist) / distRange * chartRect.width;
                float y1 = chartRect.yMax - a.HitRate * chartRect.height;
                float x2 = chartRect.xMin + (b.Distance - minDist) / distRange * chartRect.width;
                float y2 = chartRect.yMax - b.HitRate * chartRect.height;
                DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), lineColor, 2f);
            }

            foreach (var r in _seriesResults)
            {
                float px = chartRect.xMin + (r.Distance - minDist) / distRange * chartRect.width;
                float py = chartRect.yMax - r.HitRate * chartRect.height;
                EditorGUI.DrawRect(new Rect(px - 3f, py - 3f, 6f, 6f), Color.white);
            }

            var ls = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
            GUI.Label(new Rect(chartRect.xMin, chartRect.yMax - 16f, 60f, 16f), $"{minDist:F0}m", ls);
            GUI.Label(new Rect(chartRect.xMax - 40f, chartRect.yMax - 16f, 40f, 16f), $"{maxDist:F0}m", ls);
            GUI.Label(new Rect(chartRect.xMin, chartRect.yMin, 32f, 16f), "100%", ls);
            GUI.Label(new Rect(chartRect.xMin, chartRect.yMin + chartRect.height * 0.5f - 8f, 32f, 16f), "50%", ls);
        }

        // ── Runners ───────────────────────────────────────────────────────

        private void RunSingle()
        {
            _errorMessage = null;
            _seriesResults = null;
            if (!Validate(out var def)) return;

            _distance = Mathf.Max(0.1f, _distance);
            var cfg = new WeaponAccuracySimulator.SimulationConfig(def, _distance, _shotCount, _targetRadius);
            var result = WeaponAccuracySimulator.Run(cfg);
            float spreadDeg = SpreadComponent.ComputeRangePenaltyStatic(
                _distance, def.EffectiveRange, def.MaxRange, def.MaxRangeSpreadPenalty) * def.BaseSpreadDeg;
            _singleResult = new SimResult(result, spreadDeg);
        }

        private void RunSeries()
        {
            _errorMessage = null;
            _singleResult = null;
            _seriesResults = new List<SimResult>();
            if (!Validate(out var def)) return;

            var distances = ParseDistances(_seriesInput);
            if (distances == null || distances.Count == 0)
            {
                _errorMessage = "Could not parse distances. Use comma-separated numbers, e.g.: 5, 10, 20, 30";
                return;
            }

            foreach (float d in distances)
            {
                var cfg = new WeaponAccuracySimulator.SimulationConfig(def, d, _shotCount, _targetRadius);
                var result = WeaponAccuracySimulator.Run(cfg);
                float spreadDeg = SpreadComponent.ComputeRangePenaltyStatic(
                    d, def.EffectiveRange, def.MaxRange, def.MaxRangeSpreadPenalty) * def.BaseSpreadDeg;
                _seriesResults.Add(new SimResult(result, spreadDeg));
            }

            Repaint();
        }

        private void RunShotgun()
        {
            _errorMessage = null;
            _shotgunSingle = null;
            _shotgunSeries = null;
            if (!Validate(out var def)) return;

            if (_showSeries)
            {
                var distances = ParseDistances(_shotgunSeriesInput);
                if (distances == null || distances.Count == 0)
                {
                    _errorMessage = "Could not parse distances.";
                    return;
                }

                _shotgunSeries = new List<WeaponAccuracySimulator.ShotgunSimulationResult>();
                _selectedShotgunIndex = 0;
                foreach (float d in distances)
                {
                    var cfg = new WeaponAccuracySimulator.SimulationConfig(def, d, _shotCount, _targetRadius);
                    _shotgunSeries.Add(WeaponAccuracySimulator.RunShotgun(cfg));
                }
            }
            else
            {
                _distance = Mathf.Max(0.1f, _distance);
                var cfg = new WeaponAccuracySimulator.SimulationConfig(def, _distance, _shotCount, _targetRadius);
                _shotgunSingle = WeaponAccuracySimulator.RunShotgun(cfg);
            }

            Repaint();
        }

        // ── Validate / Parse ──────────────────────────────────────────────

        private bool Validate(out WeaponDefinitionData def)
        {
            def = null;
            if (_weaponDef == null)
            {
                _errorMessage = "WeaponDefinition is not assigned.";
                return false;
            }

            if (_shotCount <= 0)
            {
                _errorMessage = "Shots Count must be > 0.";
                return false;
            }

            def = _weaponDef.ToData();
            return true;
        }

        private static List<float> ParseDistances(string input)
        {
            var result = new List<float>();
            if (string.IsNullOrWhiteSpace(input)) return result;
            foreach (var part in input.Split(','))
            {
                if (float.TryParse(part.Trim(),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out float v) && v > 0f)
                    result.Add(v);
            }

            result.Sort();
            return result;
        }

        private bool IsShotgun()
            => _weaponDef != null && _weaponDef.ToData().ProjectilesPerShot > 1;

        // ── Draw helpers ──────────────────────────────────────────────────

        private static void DrawBar(Rect rect, float rate)
        {
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * rate, rect.height), HitRateColor(rate));
        }

        private static void DrawLine(Vector2 a, Vector2 b, Color color, float width)
        {
            var dir = (b - a).normalized;
            var perp = new Vector2(-dir.y, dir.x) * (width * 0.5f);
            Handles.DrawSolidRectangleWithOutline(new Vector3[]
            {
                new(a.x + perp.x, a.y + perp.y),
                new(b.x + perp.x, b.y + perp.y),
                new(b.x - perp.x, b.y - perp.y),
                new(a.x - perp.x, a.y - perp.y),
            }, color, color);
        }

        private static void DrawVerticalLine(
            Rect chartRect, float dist, float minDist, float distRange, Color color, string label)
        {
            if (dist < minDist || dist > minDist + distRange) return;
            float px = chartRect.xMin + (dist - minDist) / distRange * chartRect.width;
            EditorGUI.DrawRect(new Rect(px - 0.5f, chartRect.yMin, 1f, chartRect.height), color);
            GUI.Label(new Rect(px + 2f, chartRect.yMin, 30f, 16f), label,
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } });
        }

        private static Color HitRateColor(float rate) =>
            rate >= 0.75f ? new Color(0.3f, 0.85f, 0.3f) :
            rate >= 0.5f ? new Color(0.9f, 0.8f, 0.2f) :
            new Color(0.9f, 0.3f, 0.3f);

        // ── Internal DTO ──────────────────────────────────────────────────

        private readonly struct SimResult
        {
            public readonly float Distance;
            public readonly float SpreadDeg;
            public readonly int ShotCount;
            public readonly int Hits;
            public readonly int Misses;
            public readonly float HitRate;

            public SimResult(WeaponAccuracySimulator.SimulationResult r, float spreadDeg)
            {
                Distance = r.Distance;
                SpreadDeg = spreadDeg;
                ShotCount = r.ShotCount;
                Hits = r.Hits;
                Misses = r.Misses;
                HitRate = r.HitRate;
            }
        }
    }
}
#endif