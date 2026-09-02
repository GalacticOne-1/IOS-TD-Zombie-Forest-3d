using Galactic1.Game.Meta.Items;

namespace Galactic1.RaidLoot.Definitions
{
    /// <summary>
    /// Runtime immutable loot entry.
    /// </summary>
    public readonly struct LootEntryDefinition
    {
        public readonly ItemConfig Item;

        public readonly float Chance;

        public readonly int MinAmount;
        public readonly int MaxAmount;

        public readonly int MinDurabilityPercent;
        public readonly int MaxDurabilityPercent;

        public LootEntryDefinition(
            ItemConfig item,
            float chance,
            int minAmount,
            int maxAmount,
            int minDurabilityPercent,
            int maxDurabilityPercent)
        {
            Item = item;
            Chance = chance;

            MinAmount = minAmount;
            MaxAmount = maxAmount;

            MinDurabilityPercent = minDurabilityPercent;
            MaxDurabilityPercent = maxDurabilityPercent;
        }
    }
}