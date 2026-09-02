using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Diagnostics;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Services.Probability;
using Galactic1.RaidLoot.Services.Rules;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    public sealed class LootGenerationService
    {
        private readonly LootContainerRepository _repository;
        private readonly LocationLootProfile _profile;
        private readonly LootBalanceProfile _balanceProfile;
        private readonly DepletionCurveConfig _depletionCurve;
        private readonly ContainerDepletionService _depletion;
        private readonly LootNormalizationService _normalizer;
        private readonly LocationId _locationId;
        private readonly int _currentDay;

        private readonly Dictionary<string, LootGenerationTrace> _traces = new();

        public LootGenerationService(
            LootContainerRepository repository,
            LocationLootProfile profile,
            LootBalanceProfile balanceProfile,
            DepletionCurveConfig depletionCurve,
            ContainerDepletionService depletion,
            LootNormalizationService normalizer,
            LocationId locationId,
            int currentDay)
        {
            _repository = repository;
            _profile = profile;
            _balanceProfile = balanceProfile;
            _depletionCurve = depletionCurve;
            _depletion = depletion;
            _normalizer = normalizer;
            _locationId = locationId;
            _currentDay = currentDay;
        }

        public void OnContainerOpened(ContainerOpenedEvent e)
        {
            if (!_repository.TryGet(e.RuntimeId, out var runtime)) return;

            var containerIdStr = runtime.Id;
            var openCount = _depletion.GetOpenCount(runtime.Id);
            var stage = _depletionCurve.GetStage(openCount);
            var seed = LootSeedProvider.ComputeSeed(runtime.Id, _locationId, _currentDay, openCount);
            var rng = new SeededRandom(seed);

            var trace = new LootGenerationTrace(
                containerIdStr, seed, _currentDay, openCount,
                stage.Stage, stage.BudgetMultiplier,
                0,
                0);

            var context = new LootGenerationContext(
                runtime.Id,
                runtime.Definition.Id,
                runtime.Definition.ContainerType,
                runtime.Definition.LootTable.Id,
                _profile,
                runtime.Definition.ContainerTier,
                LootSourceType.Container);

            // Шаг 1: генерация
            var records = GenerateLoot(runtime.Definition.LootTable, context, stage, rng, trace);

            // Шаг 2: per-container caps (tier/tag/stack)
            records = NormalizationRules.Apply(records, _balanceProfile);

            // Шаг 3: raid-wide strategic caps
            var normalized = new List<LootGenerationRecord>(records.Count);
            foreach (var record in records)
            {
                var result = _normalizer.Normalize(record);
                if (result != null)
                    normalized.Add(result);
            }

            _depletion.RegisterOpen(runtime.Id);
            _traces[containerIdStr] = trace;

            runtime.StoreGeneratedItems(normalized);
            runtime.SetState(ContainerState.Open);

            EventBus<LootGeneratedEvent>.Raise(new LootGeneratedEvent(runtime.Id, normalized));
        }

        private List<LootGenerationRecord> GenerateLoot(
            LootTableDefinition table,
            LootGenerationContext context,
            DepletionCurveConfig.DepletionStageRule stage,
            SeededRandom rng,
            LootGenerationTrace trace)
        {
            var results = new List<LootGenerationRecord>();

            // ── Layer 1: Container Guaranteed ─────────────────────────────────
            foreach (var g in table.GuaranteedEntries)
            {
                if (!g.Item.HasModule<LootModule>())
                {
                    Debug.LogError($"Guaranteed loot item '{g.Item.name}' has no LootModule");
                    continue;
                }

                var category = g.Item.Classification.economyCategory;
                int rolledAmount = g.RollAmount(rng);
                var amountMul = _profile?.GetAmountMultiplier(category) ?? 1f;
                int finalAmount = Mathf.Max(1, Mathf.RoundToInt(rolledAmount * amountMul));

                trace.AddGuaranteed(new LootGenerationTrace.GuaranteedTrace
                {
                    ItemId = g.Item.Id.Guid,
                    ItemName = g.Item.Header.titleLid,
                    MinAmount = g.MinAmount,
                    MaxAmount = g.MaxAmount,
                    Amount = finalAmount,
                    Included = true
                });

                var guaranteedContext = new LootGenerationContext(
                    context.ContainerRuntimeId, 
                    context.ContainerDefinitionId,
                    context.ContainerType,
                    context.LootTableId,
                    context.Profile, 
                    context.ContainerTier,
                    LootSourceType.ContainerGuaranteed);

                results.Add(BuildRecord(g.Item, finalAmount, g.DurabilityPercent, guaranteedContext));
            }

            // ── Layer 2: Slots ────────────────────────────────────────────────
            foreach (var slot in table.Slots)
            {
                if (slot.SharedPool?.Pool == null) continue;

                for (int i = 0; i < slot.RepeatCount; i++)
                {
                    var iterationId = slot.RepeatCount > 1 ? $"{slot.SlotId}_rep_{i}" : slot.SlotId;
                    var slotTrace = new LootGenerationTrace.SlotTrace
                    {
                        SlotId = iterationId,
                        ActivationChance = slot.ActivationChance,
                    };

                    var activationRoll = rng.NextFloat();
                    slotTrace.ActivationRoll = activationRoll;
                    slotTrace.Activated = activationRoll <= slot.ActivationChance;

                    if (!slotTrace.Activated)
                    {
                        slotTrace.SkipReason = "activation roll failed";
                        trace.AddSlot(slotTrace);
                        continue;
                    }

                    var tierLimits = TierLimitResolver.Resolve(slot, stage, context.ContainerTier);
                    slotTrace.TierLimits = tierLimits;
                    var candidates = ContextFilter.Filter(slot.SharedPool.Pool, tierLimits);
                    slotTrace.CandidatesCount = candidates.Count;

                    if (candidates.Count == 0)
                    {
                        slotTrace.Included = false;
                        slotTrace.ExclusionReason = "no candidates after tier filter";
                        trace.AddSlot(slotTrace);
                        continue;
                    }

                    var pool = WeightedPool.Build(candidates, _profile);
                    var selected = WeightedSelector.Select(pool, rng, out var selectionRoll);
                    slotTrace.SelectionRoll = selectionRoll;

                    if (selected == null)
                    {
                        slotTrace.Included = false;
                        slotTrace.ExclusionReason = "weighted selection returned null";
                        trace.AddSlot(slotTrace);
                        continue;
                    }

                    slotTrace.SelectedItem = selected.Value.Source.Item.Header.titleLid;
                    slotTrace.SelectedWeight = selected.Value.AdjustedWeight;

                    // AmountMultiplier применяется внутри QuantityRoller
                    var finalAmount = QuantityRoller.Roll(selected.Value.Source, _profile, rng);
                    slotTrace.RawAmount = finalAmount;
                    slotTrace.FinalAmount = finalAmount;

                    var durability = rng.NextInt(
                        selected.Value.Source.MinDurabilityPercent,
                        selected.Value.Source.MaxDurabilityPercent);

                    results.Add(BuildRecord(selected.Value.Source.Item, finalAmount, durability, context));
                    slotTrace.Included = true;
                    trace.AddSlot(slotTrace);
                }
            }

            return results;
        }

        private static LootGenerationRecord BuildRecord(
            ItemConfig item, int amount, int durability, LootGenerationContext context)
            => new(
                item,
                amount,
                durability,
                context.ContainerRuntimeId,
                context.ContainerDefinitionId,
                context.LootTableId,
                context);

        public bool TryGetTrace(string key, out LootGenerationTrace trace)
            => _traces.TryGetValue(key, out trace);

        public void ClearTraces() => _traces.Clear();
    }
}