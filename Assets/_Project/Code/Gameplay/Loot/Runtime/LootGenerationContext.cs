using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Enums;

namespace Galactic1.RaidLoot.Runtime
{
    public sealed class LootGenerationContext
    {
        public string ContainerRuntimeId { get; }
        public LootContainerId ContainerDefinitionId { get; }
        public ContainerType ContainerType { get; }
        public LootTableId LootTableId { get; }
        public LocationLootProfile Profile { get; }
        public Tier ContainerTier { get; }
        public LootSourceType SourceType { get; }

        public LootGenerationContext(
            string containerRuntimeId,
            LootContainerId containerDefinitionId,
            ContainerType containerType,
            LootTableId lootTableId,
            LocationLootProfile profile,
            Tier containerTier,
            LootSourceType sourceType = LootSourceType.Container)
        {
            ContainerRuntimeId = containerRuntimeId;
            ContainerDefinitionId = containerDefinitionId;
            ContainerType = containerType;
            LootTableId = lootTableId;
            Profile = profile;
            ContainerTier = containerTier;
            SourceType = sourceType;
        }
    }
}