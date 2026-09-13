using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Mobile.EventBus
{
    
    /// <summary>
    /// Событие переноса предета из inbox to inventory
    /// </summary>
    public readonly struct InboxItemColectedEvent : IEvent
    {
        public readonly RuntimeId ItemId;
        public readonly int Amount;

        public InboxItemColectedEvent(RuntimeId itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
    
    
    
    
    
    // =========================================================
    // LOOT CONTAINER OPENED
    // =========================================================

    /// <summary>
    /// Published after a loot container has been successfully opened.
    ///
    /// The container runtime state must already indicate that the container
    /// is opened before this event is raised.
    ///
    /// This event means that the player gained access to the container loot.
    /// It does not mean that any item has already been collected.
    /// </summary>
    public readonly struct LootContainerOpenedEvent : IEvent
    {
        /// <summary>
        /// Runtime identity of the opened container.
        /// </summary>
        public readonly RuntimeId ContainerId;

        /// <summary>
        /// Inventory source representing the opened container loot.
        /// </summary>
        public readonly IInventorySource LootSource;

        /// <summary>
        /// Inventory source of the unit/player that opened the container.
        /// Can be null when the opening operation has no direct owner source.
        /// </summary>
        public readonly IInventorySource OpenerSource;

        public LootContainerOpenedEvent(
            RuntimeId containerId,
            IInventorySource lootSource,
            IInventorySource openerSource)
        {
            ContainerId = containerId;
            LootSource = lootSource;
            OpenerSource = openerSource;
        }
    }


    // =========================================================
    // LOOT CONTAINER CLOSED
    // =========================================================

    /// <summary>
    /// Published after the loot container UI or interaction session
    /// has been closed.
    ///
    /// This event represents the end of the current loot interaction.
    /// It does not mean that the container has been fully looted.
    /// </summary>
    public readonly struct LootContainerClosedEvent : IEvent
    {
        /// <summary>
        /// Runtime identity of the closed container.
        /// </summary>
        public readonly RuntimeId ContainerId;

        /// <summary>
        /// Inventory source representing the container loot.
        /// </summary>
        public readonly IInventorySource LootSource;

        public LootContainerClosedEvent(
            RuntimeId containerId,
            IInventorySource lootSource)
        {
            ContainerId = containerId;
            LootSource = lootSource;
        }
    }


    // =========================================================
    // SINGLE LOOT ITEM COLLECTED
    // =========================================================

    /// <summary>
    /// Published after an item or item stack has been successfully
    /// transferred from a loot source into another inventory source.
    ///
    /// This is the canonical gameplay event for "the player collected loot".
    ///
    /// The item must already be present in the destination inventory when
    /// this event is raised.
    /// </summary>
    public readonly struct LootItemCollectedEvent : IEvent
    {
        /// <summary>
        /// Runtime identity of the container from which the item was taken.
        /// </summary>
        public readonly RuntimeId ContainerId;

        /// <summary>
        /// Source inventory from which the item was collected.
        /// </summary>
        public readonly IInventorySource Source;

        /// <summary>
        /// Destination inventory to which the item was transferred.
        /// </summary>
        public readonly IInventorySource Destination;

        /// <summary>
        /// Runtime item identifier.
        /// </summary>
        public readonly RuntimeId ItemId;

        /// <summary>
        /// Number of items transferred by this operation.
        /// </summary>
        public readonly int Amount;

        /// <summary>
        /// Source slot index before or during the transfer.
        /// </summary>
        public readonly int SourceSlotIndex;

        /// <summary>
        /// Destination slot index used by the transfer.
        ///
        /// A value of -1 means that the destination slot is not stable,
        /// was not exposed by the transfer API, or multiple slots were used.
        /// </summary>
        public readonly int DestinationSlotIndex;

        public LootItemCollectedEvent(
            RuntimeId containerId,
            IInventorySource source,
            IInventorySource destination,
            RuntimeId itemId,
            int amount,
            int sourceSlotIndex,
            int destinationSlotIndex = -1)
        {
            ContainerId = containerId;
            Source = source;
            Destination = destination;
            ItemId = itemId;
            Amount = amount;
            SourceSlotIndex = sourceSlotIndex;
            DestinationSlotIndex = destinationSlotIndex;
        }
    }


    // =========================================================
    // LOOT ALL COLLECTED
    // =========================================================

    /// <summary>
    /// Published after a "collect all" operation has finished.
    ///
    /// The event is published once per operation, not once per item.
    /// Individual successfully transferred items should still publish
    /// LootItemCollectedEvent.
    /// </summary>
    public readonly struct LootAllCollectedEvent : IEvent
    {
        /// <summary>
        /// Runtime identity of the container.
        /// </summary>
        public readonly RuntimeId ContainerId;

        /// <summary>
        /// Source inventory representing the container loot.
        /// </summary>
        public readonly IInventorySource Source;

        /// <summary>
        /// Destination inventory receiving the collected loot.
        /// </summary>
        public readonly IInventorySource Destination;

        /// <summary>
        /// Number of individual item transfer operations that succeeded.
        /// </summary>
        public readonly int CollectedItemCount;

        /// <summary>
        /// Total number of item units transferred.
        /// </summary>
        public readonly int CollectedAmount;

        public LootAllCollectedEvent(
            RuntimeId containerId,
            IInventorySource source,
            IInventorySource destination,
            int collectedItemCount,
            int collectedAmount)
        {
            ContainerId = containerId;
            Source = source;
            Destination = destination;
            CollectedItemCount = collectedItemCount;
            CollectedAmount = collectedAmount;
        }
    }


    // =========================================================
    // LOOT CONTAINER FULLY LOOTED
    // =========================================================

    /// <summary>
    /// Published when a loot container has no more collectible items.
    ///
    /// This event is different from LootContainerClosedEvent:
    ///
    /// - Closed means the player ended the current interaction.
    /// - Looted means the container has been completely emptied.
    ///
    /// The container runtime state must already indicate that no collectible
    /// loot remains when this event is raised.
    /// </summary>
    public readonly struct LootContainerLootedEvent : IEvent
    {
        /// <summary>
        /// Runtime identity of the fully looted container.
        /// </summary>
        public readonly RuntimeId ContainerId;

        /// <summary>
        /// Inventory source representing the container loot.
        /// </summary>
        public readonly IInventorySource LootSource;

        /// <summary>
        /// Inventory source of the unit/player that looted the container.
        /// </summary>
        public readonly IInventorySource CollectorSource;

        public LootContainerLootedEvent(
            RuntimeId containerId,
            IInventorySource lootSource,
            IInventorySource collectorSource)
        {
            ContainerId = containerId;
            LootSource = lootSource;
            CollectorSource = collectorSource;
        }
    }
}