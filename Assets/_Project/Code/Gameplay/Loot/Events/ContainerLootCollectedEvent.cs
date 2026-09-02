using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Events
{
    /// <summary>
    /// Entire container loot collected.
    ///
    /// One event per container.
    /// Intended for UI feedback.
    /// </summary>
    public readonly struct ContainerLootCollectedEvent : IEvent
    {
        public readonly string ContainerId;

        public readonly IReadOnlyList<LootGenerationRecord> Loot;

        public ContainerLootCollectedEvent(
            string containerId,
            IReadOnlyList<LootGenerationRecord> loot)
        {
            ContainerId = containerId;
            Loot = loot;
        }
    }
}