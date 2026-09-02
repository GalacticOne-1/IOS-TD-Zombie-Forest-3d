
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Отвечает ТОЛЬКО за количество — не за вероятность выбора.
    /// AmountModifier из LocationLootProfile применяется здесь.
    /// </summary>
    public static class QuantityRoller
    {
        public static int Roll(
            LootWeightedEntry entry,
            LocationLootProfile profile,
            SeededRandom rng)
        {
            var raw = rng.NextInt(entry.MinAmount, entry.MaxAmount);

            // AmountMultiplier — только quantity, не probability
            var modifier = profile?.GetAmountMultiplier(entry.Item.Classification.economyCategory) ?? 1f;

            return Mathf.Max(1, UnityEngine.Mathf.RoundToInt(raw * modifier));
        }
    }
}