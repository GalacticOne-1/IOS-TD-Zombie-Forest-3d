using Galactic1.Core.Enums;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Enums;

namespace Galactic1.RaidLoot.Definition
{
    public sealed class LootContainerDefinition
    {
        public LootContainerId Id { get; }
        public ContainerType ContainerType { get; }
        public LootTableDefinition LootTable { get; }
        public Tier ContainerTier { get; }
        public float OpenRadius { get; }
        public float OpenTimerDelay { get; }
        public LootVisualId VisualId { get; }

        public LootContainerDefinition(
            LootContainerId id,
            ContainerType containerType,
            LootTableDefinition lootTable,
            Tier containerTier,
            float openRadius,
            float openTimerDelay,
            LootVisualId visualId)
        {
            Id = id;
            ContainerType = containerType;
            LootTable = lootTable;
            ContainerTier = containerTier;
            OpenRadius = openRadius;
            OpenTimerDelay = openTimerDelay;
            VisualId = visualId;
        }
    }
}