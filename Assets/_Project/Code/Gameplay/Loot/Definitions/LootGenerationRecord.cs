using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Runtime
{
    public sealed class LootGenerationRecord
    {
        public ItemConfig Item { get; }
        public int Amount { get; }
        public int Durability { get; }


        public string ContainerRuntimeId { get; }
        public LootContainerId ContainerDefinitionId { get; }
        public LootTableId SourceLootTableId { get; }
        public string SourceEntryId => Item.Id.Guid;
        public LootGenerationContext Context { get; }

        public LootGenerationRecord(
            ItemConfig item,
            int amount,
            int durability,
            string containerRuntimeId,
            LootContainerId containerDefinitionId,
            LootTableId sourceLootTableId,
            LootGenerationContext context)
        {
            Item = item;
            Amount = amount;
            Durability = durability;
            ContainerRuntimeId = containerRuntimeId;
            ContainerDefinitionId = containerDefinitionId;
            SourceLootTableId = sourceLootTableId;
            Context = context;
        }
    }
}