using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Events
{
    /// <summary>
    /// Published by LootAutoPickupService for each record moved to the buffer.
    /// Consumed by WorldSpaceLootFeedback to show floating "+Iron x3" text.
    /// </summary>
    public sealed class LootAutoPickedEvent : IEvent
    {
        public LootGenerationRecord Record { get; }
        public LootContainerId ContainerId { get; }

        public LootAutoPickedEvent(LootGenerationRecord record, LootContainerId containerId)
        {
            Record = record;
            ContainerId = containerId;
            DLog.Alert("Loot auto picked event");
        }
    }
}