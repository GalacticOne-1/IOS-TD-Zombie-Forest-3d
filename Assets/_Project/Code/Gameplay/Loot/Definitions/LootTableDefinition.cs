
using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definitions;

namespace Galactic1.RaidLoot.Definition
{
    public sealed class LootTableDefinition
    {
        public LootTableId Id { get; }
        public IReadOnlyList<LootSlotConfig> Slots { get; }
        public IReadOnlyList<LootGuaranteedEntry> GuaranteedEntries { get; }


        public LootTableDefinition(
            LootTableId id,
            IReadOnlyList<LootSlotConfig> slots,
            IReadOnlyList<LootGuaranteedEntry> guaranteedEntries)
        {
            Id = id;
            Slots = slots ?? System.Array.Empty<LootSlotConfig>();
            GuaranteedEntries = guaranteedEntries ?? System.Array.Empty<LootGuaranteedEntry>();
        }
    }
}