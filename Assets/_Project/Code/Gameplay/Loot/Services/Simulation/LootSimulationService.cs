using System;
using System.Collections.Generic;
using System.Text;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Services;
using Galactic1.RaidLoot.Services.Probability;
using Galactic1.RaidLoot.Services.Rules;
using UnityEngine;

namespace Galactic1.RaidLoot.Diagnostics
{
    public sealed class LootSimulationService
    {
        // ── Per-item stats accumulator ────────────────────────────────────────

        internal sealed class ItemStats
        {
            public int Appearances;
            public int TotalAmount;
            public int MinRoll = int.MaxValue;
            public int MaxRoll = int.MinValue;
            public bool IsStrategic;

            // для std deviation: накапливаем сумму квадратов
            private long _sumOfSquares;

            public void Record(int amount)
            {
                Appearances++;
                TotalAmount += amount;
                _sumOfSquares += (long)amount * amount;
                if (amount < MinRoll) MinRoll = amount;
                if (amount > MaxRoll) MaxRoll = amount;
            }

            public float AverageAmount => Appearances == 0 ? 0f : TotalAmount / (float)Appearances;

            public float StdDeviation()
            {
                if (Appearances < 2) return 0f;
                var mean = AverageAmount;
                var variance = _sumOfSquares / (float)Appearances - mean * mean;
                return (float)Math.Sqrt(Math.Max(0f, variance));
            }
        }

        public sealed class SimulationReport
        {
            public int Iterations { get; set; }
            public LootSimulationMode Mode { get; set; }

            public Dictionary<string, int> ItemFrequency { get; } = new();
            public Dictionary<string, int> ItemTotalAmount { get; } = new();
            public Dictionary<Tier, int> TierDistribution { get; } = new();
            public Dictionary<string, int> SlotUtilization { get; } = new();
            public int TotalItemsGenerated { get; set; }

            // internal — читается только FormatReport
            internal Dictionary<string, ItemStats> Stats { get; } = new();
        }

        private readonly LootBalanceProfile _balanceProfile;
        private readonly DepletionCurveConfig _depletionCurve;

        public LootSimulationService(
            LootBalanceProfile balanceProfile,
            DepletionCurveConfig depletionCurve)
        {
            _balanceProfile = balanceProfile;
            _depletionCurve = depletionCurve;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public SimulationReport RunDeterministic(
            LootTableDefinition table,
            Tier containerTier,
            int seed = 12345,
            int iterations = 1000,
            int openCountForStage = 0)
        {
            var report = new SimulationReport { Iterations = iterations, Mode = LootSimulationMode.Deterministic };
            var stage = _depletionCurve.GetStage(openCountForStage);

            for (var i = 0; i < iterations; i++)
                RunIteration(table, containerTier, stage, new SeededRandom(seed), report);

            return report;
        }

        public SimulationReport RunStatistical(
            LootTableDefinition table,
            Tier containerTier,
            int baseSeed = 0,
            int iterations = 10000,
            int openCountForStage = 0)
        {
            var report = new SimulationReport { Iterations = iterations, Mode = LootSimulationMode.Statistical };
            var stage = _depletionCurve.GetStage(openCountForStage);

            for (var i = 0; i < iterations; i++)
                RunIteration(table, containerTier, stage, new SeededRandom(baseSeed + i), report);

            return report;
        }

        public string ValidateRandomness(
            LootTableDefinition table,
            Tier containerTier,
            int probeCount = 1000)
        {
            var stage = _depletionCurve.GetStage(0);
            var uniqueHashes = new HashSet<int>();

            for (var i = 0; i < probeCount; i++)
            {
                var result = new StringBuilder();
                RunIterationToString(table, containerTier, stage, new SeededRandom(i), result);
                uniqueHashes.Add(result.ToString().GetHashCode());
            }

            var uniqueCount = uniqueHashes.Count;
            var sb = new StringBuilder();
            sb.AppendLine("=== RNG Validation ===");
            sb.AppendLine($"Probes     : {probeCount}");
            sb.AppendLine($"Unique     : {uniqueCount}");
            sb.AppendLine($"Uniqueness : {uniqueCount * 100f / probeCount:F1}%");

            if (uniqueCount <= 1)
                sb.AppendLine(
                    "WARNING: Simulation uses identical seeds. " +
                    "Distribution analysis is invalid.");
            else if (uniqueCount < probeCount * 0.5f)
                sb.AppendLine(
                    "WARNING: Low uniqueness — possible seed collision or " +
                    "table too small to produce varied results.");
            else
                sb.AppendLine("OK: RNG produces varied results.");

            return sb.ToString();
        }

        // ── Core ──────────────────────────────────────────────────────────────

        private static void RunIteration(
            LootTableDefinition table,
            Tier containerTier,
            DepletionCurveConfig.DepletionStageRule stage,
            SeededRandom rng,
            SimulationReport report)
        {
            var iterItems = 0;

            foreach (var g in table.GuaranteedEntries)
            {
                var amount = g.RollAmount(rng);
                RecordItem(g.Item.Header.titleLid, amount, g.Item, report);
                iterItems++;
            }

            foreach (var slot in table.Slots)
            {
                if (slot.SharedPool?.Pool == null) continue;

                for (int rep = 0; rep < slot.RepeatCount; rep++)
                {
                    if (rng.NextFloat() > slot.ActivationChance) continue;

                    var tierLimits = TierLimitResolver.Resolve(slot, stage, containerTier);
                    var candidates = ContextFilter.Filter(slot.SharedPool.Pool, tierLimits);
                    if (candidates.Count == 0) continue;

                    var pool = WeightedPool.Build(candidates, null);
                    var selected = WeightedSelector.Select(pool, rng, out _);
                    if (selected == null) continue;

                    var amount = QuantityRoller.Roll(selected.Value.Source, null, rng);
                    RecordItem(selected.Value.Source.Item.Header.titleLid, amount,
                        selected.Value.Source.Item, report);

                    var slotKey = slot.RepeatCount > 1 ? $"{slot.SlotId}_rep_{rep}" : slot.SlotId;
                    if (!report.SlotUtilization.ContainsKey(slotKey))
                        report.SlotUtilization[slotKey] = 0;
                    report.SlotUtilization[slotKey]++;

                    iterItems++;
                }
            }

            report.TotalItemsGenerated += iterItems;
        }

        private static void RunIterationToString(
            LootTableDefinition table,
            Tier containerTier,
            DepletionCurveConfig.DepletionStageRule stage,
            SeededRandom rng,
            StringBuilder result)
        {
            foreach (var g in table.GuaranteedEntries)
                result.Append($"{g.Item.Id.Guid}:{g.RollAmount(rng)};");

            foreach (var slot in table.Slots)
            {
                if (slot.SharedPool?.Pool == null) continue;
                for (int rep = 0; rep < slot.RepeatCount; rep++)
                {
                    if (rng.NextFloat() > slot.ActivationChance)
                    {
                        result.Append("skip;");
                        continue;
                    }

                    var candidates = ContextFilter.Filter(
                        slot.SharedPool.Pool,
                        TierLimitResolver.Resolve(slot, stage, containerTier));
                    if (candidates.Count == 0)
                    {
                        result.Append("empty;");
                        continue;
                    }

                    var selected = WeightedSelector.Select(WeightedPool.Build(candidates, null), rng, out _);
                    if (selected == null)
                    {
                        result.Append("null;");
                        continue;
                    }

                    result.Append(
                        $"{selected.Value.Source.Item.Id.Guid}:{QuantityRoller.Roll(selected.Value.Source, null, rng)};");
                }
            }
        }

        private static void RecordItem(
            string name,
            int amount,
            ItemConfig item,
            SimulationReport report)
        {
            // Общая статистика
            if (!report.ItemFrequency.ContainsKey(name))
            {
                report.ItemFrequency[name] = 0;
                report.ItemTotalAmount[name] = 0;
            }

            report.ItemFrequency[name]++;
            report.ItemTotalAmount[name] += amount;

            var lootModule = item.LootModule;

            var tier = item.Classification.tier;
            if (!report.TierDistribution.ContainsKey(tier))
                report.TierDistribution[tier] = 0;
            report.TierDistribution[tier]++;

            // Детальная статистика (min/max/avg/stddev) — все предметы,
            // но в отчёт выводится только для стратегических ресурсов
            if (!report.Stats.ContainsKey(name))
                report.Stats[name] = new ItemStats { IsStrategic = lootModule.IsStrategicResource };
            report.Stats[name].Record(amount);
        }

        // ── Formatting ────────────────────────────────────────────────────────

        public static string FormatReport(SimulationReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Simulation Report ({report.Iterations} iterations, {report.Mode}) ===");
            sb.AppendLine($"Total items generated : {report.TotalItemsGenerated}");
            sb.AppendLine();

            // ── Item Frequency ────────────────────────────────────────────────
            sb.AppendLine("── Item Frequency ──");
            foreach (var kv in report.ItemFrequency)
            {
                var total = report.ItemTotalAmount[kv.Key];
                var chance = kv.Value * 100f / report.Iterations;
                var avgAmount = total / (float)kv.Value;
                sb.AppendLine(
                    $"  {kv.Key,-30}" +
                    $"  appeared={kv.Value,6}/{report.Iterations}" +
                    $"  chance={chance,5:F1}%" +
                    $"  avg_amount={avgAmount,5:F1}");
            }

            // ── Strategic Resources ───────────────────────────────────────────
            var hasStrategic = false;
            foreach (var kv in report.Stats)
                if (kv.Value.IsStrategic)
                {
                    hasStrategic = true;
                    break;
                }

            if (hasStrategic)
            {
                sb.AppendLine();
                sb.AppendLine("── Strategic Resources ──");
                foreach (var kv in report.Stats)
                {
                    if (!kv.Value.IsStrategic) continue;
                    var s = kv.Value;
                    var chance = s.Appearances * 100f / report.Iterations;
                    sb.AppendLine($"  {kv.Key}");
                    sb.AppendLine($"    Appeared  : {s.Appearances} / {report.Iterations}  ({chance:F1}%)");
                    sb.AppendLine($"    Min Roll  : {(s.Appearances > 0 ? s.MinRoll.ToString() : "—")}");
                    sb.AppendLine($"    Max Roll  : {(s.Appearances > 0 ? s.MaxRoll.ToString() : "—")}");
                    sb.AppendLine($"    Avg Roll  : {s.AverageAmount:F1}");
                    sb.AppendLine($"    Std Dev   : {s.StdDeviation():F2}");
                }
            }

            // ── Tier Distribution ─────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine("── Tier Distribution ──");
            foreach (var kv in report.TierDistribution)
                sb.AppendLine($"  {kv.Key}: {kv.Value}");

            // ── Slot Utilization ──────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine("── Slot Utilization ──");
            foreach (var kv in report.SlotUtilization)
                sb.AppendLine(
                    $"  {kv.Key,-30}  activations={kv.Value,6}  rate={kv.Value * 100f / report.Iterations:F1}%");

            return sb.ToString();
        }
    }
}