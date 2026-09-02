using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Definition
{
    public sealed class LocationLootProfile
    {
        private readonly Dictionary<LootEconomyCategory, LootMultiplierData> _multipliers;

        public LocationId LocationId { get; }

        public LocationLootProfile(
            LocationId locationId,
            IEnumerable<LootMultiplierEntry> entries)
        {
            LocationId = locationId;

            _multipliers = entries.ToDictionary(
                x => x.Category,
                x => new LootMultiplierData(
                    x.WeightMultiplier,
                    x.AmountMultiplier));
        }

        public float GetWeightMultiplier(LootEconomyCategory category)
        {
            return _multipliers.TryGetValue(category, out var data)
                ? data.WeightMultiplier
                : 1f;
        }

        public float GetAmountMultiplier(LootEconomyCategory category)
        {
            return _multipliers.TryGetValue(category, out var data)
                ? data.AmountMultiplier
                : 1f;
        }

        private readonly struct LootMultiplierData
        {
            public readonly float WeightMultiplier;
            public readonly float AmountMultiplier;

            public LootMultiplierData(
                float weightMultiplier,
                float amountMultiplier)
            {
                WeightMultiplier = weightMultiplier;
                AmountMultiplier = amountMultiplier;
            }
        }
    }
}