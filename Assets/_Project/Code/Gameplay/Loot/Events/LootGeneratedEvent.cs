using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Events
{
    /// <summary>
    /// Published by LootGenerationService after loot has been rolled and
    /// stored in LootContainerRuntime. Triggers LootAutoPickupService.
    /// </summary>
    public sealed class LootGeneratedEvent : IEvent
    {
        public string RuntimeId { get; }
        public IReadOnlyList<LootGenerationRecord> GeneratedItems { get; }

        public LootGeneratedEvent(
            string runtimeId,
            IReadOnlyList<LootGenerationRecord> generatedItems)
        {
            RuntimeId = runtimeId;
            GeneratedItems = generatedItems;
            DLog.Alert("Loot generated");
        }
    }
}