using System.Collections.Generic;
using System.Text;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Services.Rules;

namespace Galactic1.RaidLoot.Diagnostics
{
    /// <summary>
    /// Полная запись одной генерации лута.
    /// Позволяет ответить на вопрос: "Почему выпал/не выпал этот предмет?"
    /// Создаётся в LootGenerationService / LocationGuaranteedLootGenerationService,
    /// читается LootExplanationService.
    /// </summary>
    public sealed class LootGenerationTrace
    {
        public string ContainerId { get; }
        public int Seed { get; }
        public int DayNumber { get; }
        public int OpenCount { get; }

        public DepletionStage DepletionStage { get; }
        public float BudgetMultiplier { get; }
        public int InitialBudget { get; }
        public int InitialCapacity { get; }

        public IReadOnlyList<SlotTrace> SlotTraces { get; }
        public IReadOnlyList<GuaranteedTrace> GuaranteedTraces { get; }

        // Task 10: трассировка location-guaranteed слоя
        public IReadOnlyList<LocationGuaranteedTrace> LocationGuaranteedTraces { get; }

        private readonly List<SlotTrace> _slots = new();
        private readonly List<GuaranteedTrace> _guaranteed = new();
        private readonly List<LocationGuaranteedTrace> _locationGuaranteed = new();

        public LootGenerationTrace(
            string containerId,
            int seed,
            int dayNumber,
            int openCount,
            DepletionStage depletionStage,
            float budgetMultiplier,
            int initialBudget,
            int initialCapacity)
        {
            ContainerId = containerId;
            Seed = seed;
            DayNumber = dayNumber;
            OpenCount = openCount;
            DepletionStage = depletionStage;
            BudgetMultiplier = budgetMultiplier;
            InitialBudget = initialBudget;
            InitialCapacity = initialCapacity;

            SlotTraces = _slots;
            GuaranteedTraces = _guaranteed;
            LocationGuaranteedTraces = _locationGuaranteed;
        }

        internal void AddSlot(SlotTrace t) => _slots.Add(t);
        internal void AddGuaranteed(GuaranteedTrace t) => _guaranteed.Add(t);
        internal void AddLocationGuaranteed(LocationGuaranteedTrace t) => _locationGuaranteed.Add(t);

        // ── Nested trace types ────────────────────────────────────────────────

        public sealed class SlotTrace
        {
            public string SlotId { get; set; }
            public float ActivationRoll { get; set; }
            public float ActivationChance { get; set; }
            public bool Activated { get; set; }
            public string SkipReason { get; set; }
            public int CandidatesCount { get; set; }
            public string SelectedItem { get; set; }
            public float SelectionRoll { get; set; }
            public float SelectedWeight { get; set; }
            public int RawAmount { get; set; }
            public int FinalAmount { get; set; }
            public int ValuePerUnit { get; set; }
            public int SlotCost { get; set; }
            public int BudgetBefore { get; set; }
            public int BudgetAfter { get; set; }
            public int CapacityBefore { get; set; }
            public bool Included { get; set; }
            public string ExclusionReason { get; set; }
            public TierLimitResolver.TierLimits TierLimits { get; set; }
        }

        public sealed class GuaranteedTrace
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public int Amount { get; set; }
            public int MinAmount { get; set; }
            public int MaxAmount { get; set; }
            public bool Included { get; set; }
        }

        // Task 10: отдельный trace для location-guaranteed слоя
        /// <summary>
        /// Запись для отладки экономики гарантированных ресурсов локации.
        /// Пример: food_supply base=20 * multiplier=1.5 → final=30
        /// </summary>
        public sealed class LocationGuaranteedTrace
        {
            /// <summary>GUID предмета из ItemConfig.</summary>
            public string ItemId { get; set; }

            /// <summary>Локализационный ключ названия.</summary>
            public string ItemName { get; set; }

            /// <summary>Количество, выпавшее из RNG до умножения на AmountMultiplier.</summary>
            public int BaseAmount { get; set; }

            /// <summary>AmountMultiplier из LocationLootProfile для категории этого предмета.</summary>
            public float AmountMultiplier { get; set; }
            
            public int LocationAdjustedAmount { get; set; }

            /// <summary>Итоговое количество после применения мультипликатора.</summary>
            public int FinalAmount { get; set; }
        }

        // ── Human-readable output ─────────────────────────────────────────────

        public string ToReadableString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== LootGenerationTrace ===");
            sb.AppendLine($"Container : {ContainerId}");
            sb.AppendLine($"Seed      : {Seed}");
            sb.AppendLine($"Day       : {DayNumber}  |  Open #{OpenCount}");
            sb.AppendLine($"Depletion : {DepletionStage}  (budget x{BudgetMultiplier:F2})");
            sb.AppendLine($"Budget    : {InitialBudget}  |  Capacity: {InitialCapacity}");
            sb.AppendLine();

            if (_locationGuaranteed.Count > 0)
            {
                sb.AppendLine("── Location Guaranteed ──");
                foreach (var lg in _locationGuaranteed)
                    sb.AppendLine(
                        $"  {lg.ItemName}  base={lg.BaseAmount}  x{lg.AmountMultiplier:F2}  → final={lg.FinalAmount}");
                sb.AppendLine();
            }

            if (_guaranteed.Count > 0)
            {
                sb.AppendLine("── Container Guaranteed ──");
                foreach (var g in _guaranteed)
                    sb.AppendLine(
                        $"  [{(g.Included ? "OK" : "SKIP-CAP")}] {g.ItemName} x{g.Amount} [{g.MinAmount}/{g.MaxAmount}]");
                sb.AppendLine();
            }

            sb.AppendLine("── Slots ──");
            foreach (var s in _slots)
            {
                sb.AppendLine($"  Slot [{s.SlotId}]");
                sb.AppendLine(
                    $"    activation: roll={s.ActivationRoll:F3} vs {s.ActivationChance:F3} → {(s.Activated ? "PASS" : "FAIL")}");

                if (!s.Activated)
                {
                    sb.AppendLine($"    → SKIP ({s.SkipReason})");
                    continue;
                }

                sb.AppendLine($"    tier limits: [{s.TierLimits.Min}–{s.TierLimits.Max}]");
                sb.AppendLine($"    candidates: {s.CandidatesCount}");

                if (s.CandidatesCount == 0)
                {
                    sb.AppendLine("    → SKIP (no candidates after filter)");
                    continue;
                }

                sb.AppendLine(
                    $"    selected: {s.SelectedItem}  weight={s.SelectedWeight:F3}  roll={s.SelectionRoll:F3}");
                sb.AppendLine($"    amount: raw={s.RawAmount} → final={s.FinalAmount}");
                sb.AppendLine($"    budget: {s.BudgetBefore} → {s.BudgetAfter}  (cost={s.SlotCost})");
                sb.AppendLine($"    capacity: {s.CapacityBefore} → {s.CapacityBefore - 1}");
                sb.AppendLine(s.Included ? "    → INCLUDED" : $"    → EXCLUDED ({s.ExclusionReason})");
            }

            return sb.ToString();
        }
    }
}