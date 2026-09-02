using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Diagnostics;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Runtime;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Генерирует гарантированные ресурсы локации (food, water, fuel и т.п.)
    /// ОДИН РАЗ при старте рейда — до открытия любого контейнера.
    ///
    /// Поток:
    ///   Generate → LootNormalizationService.Normalize() → buffer.AddItem()
    /// </summary>
    public sealed class LocationGuaranteedLootGenerationService
    {
        private readonly LocationGuaranteedProfile _profile;
        private readonly LocationLootProfile _lootProfile;
        private readonly LootNormalizationService _normalizer;
        private readonly LocationId _locationId;
        private readonly int _currentDay;

        private LootGenerationTrace _trace;
        public LootGenerationTrace Trace => _trace;

        public LocationGuaranteedLootGenerationService(
            LocationGuaranteedProfile profile,
            LocationLootProfile lootProfile,
            LootNormalizationService normalizer,
            LocationId locationId,
            int currentDay)
        {
            _profile = profile;
            _lootProfile = lootProfile;
            _normalizer = normalizer;
            _locationId = locationId;
            _currentDay = currentDay;
        }

        public void Generate(RaidLootBuffer buffer)
        {
            _trace = new LootGenerationTrace(
                containerId: "location_guaranteed",
                seed: LootSeedProvider.ComputeLocationGuaranteedSeed(
                    _locationId,
                    _currentDay),
                dayNumber: _currentDay,
                openCount: 0,
                depletionStage: DepletionStage.Full,
                budgetMultiplier: 1f,
                initialBudget: int.MaxValue,
                initialCapacity: int.MaxValue);

            if (_profile?.Entries == null || _profile.Entries.Count == 0)
                return;

            var seed = LootSeedProvider.ComputeLocationGuaranteedSeed(
                _locationId,
                _currentDay);

            var rng = new SeededRandom(seed);

            foreach (var entry in _profile.Entries)
            {
                if (entry.Item == null)
                    continue;

                var category = entry.Item.Classification.economyCategory;

                // =====================================================
                // Stage 1: RNG
                // =====================================================

                var baseAmount = entry.RollAmount(rng);

                // =====================================================
                // Stage 2: Location Economy Matrix
                // =====================================================

                var amountMultiplier =
                    _lootProfile?.GetAmountMultiplier(category) ?? 1f;

                var locationAdjustedAmount =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(baseAmount * amountMultiplier));

                var context = new LootGenerationContext(
                    "",
                    default,
                    ContainerType.None,
                    null,
                    _lootProfile,
                    Tier.T1,
                    LootSourceType.LocationGuaranteed);

                var generatedRecord = new LootGenerationRecord(
                    entry.Item,
                    locationAdjustedAmount,
                    entry.DurabilityPercent,
                    "",
                    null,
                    null,
                    context);

                // =====================================================
                // Stage 3: Raid Economy Matrix
                // =====================================================

                var normalizedRecord =
                    _normalizer.Normalize(generatedRecord);

                var finalAmount =
                    normalizedRecord?.Amount ?? 0;

                // =====================================================
                // Trace
                // =====================================================

                _trace.AddLocationGuaranteed(
                    new LootGenerationTrace.LocationGuaranteedTrace
                    {
                        ItemId = entry.Item.Id.Guid,
                        ItemName = entry.Item.Header.titleLid,

                        BaseAmount = baseAmount,

                        AmountMultiplier = amountMultiplier,

                        LocationAdjustedAmount =
                            locationAdjustedAmount,

                        FinalAmount = finalAmount
                    });

                // Полностью вырезан Raid Economy Matrix
                if (normalizedRecord == null)
                    continue;

                if (normalizedRecord.Amount <= 0)
                    continue;

                // =====================================================
                // Final Buffer
                // =====================================================

                buffer.AddItem(normalizedRecord);
            }
        }
    }
}