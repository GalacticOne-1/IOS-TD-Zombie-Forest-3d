using System.Collections.Generic;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;

namespace Galactic1.RaidLoot.Services.Probability
{
    public sealed class WeightedPool
    {
        public readonly struct Entry
        {
            public readonly LootWeightedEntry Source;
            public readonly float AdjustedWeight;

            public Entry(LootWeightedEntry source, float adjustedWeight)
            {
                Source = source;
                AdjustedWeight = adjustedWeight;
            }
        }

        public IReadOnlyList<Entry> Entries { get; }
        public float TotalWeight { get; }

        private WeightedPool(List<Entry> entries, float totalWeight)
        {
            Entries = entries;
            TotalWeight = totalWeight;
        }

        public static WeightedPool Build(
            List<LootWeightedEntry> candidates,
            LocationLootProfile profile)
        {
            var entries = new List<Entry>(candidates.Count);
            var totalWeight = 0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];

                // WeightMultiplier — только spawn probability, не quantity
                var weightMul = profile?.GetWeightMultiplier(c.Item.Classification.economyCategory) ?? 1f;
                var adjusted = c.Weight * weightMul;

                totalWeight += adjusted;
                entries.Add(new Entry(c, adjusted));
            }

            return new WeightedPool(entries, totalWeight);
        }
    }
}