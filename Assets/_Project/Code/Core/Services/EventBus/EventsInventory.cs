using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Core.Enums;

namespace Galactic1.Mobile.EventBus
{
    
    
    /// <summary>Клик по одной из главных вкладок панели инвентаря (campButton/squadButton/
    /// logisticsButton) — см. InventoryManagementPanelState.SelectMainTab. Отдельный канал
    /// от InventorySquadExtraTabSelectedEvent: squadButton и squadExtraTransport могут
    /// устанавливать одно и то же значение InventoryGameplayMode.Transport_SquadOnly, но это
    /// два разных UI-элемента — guidance обязан различать, по какому именно кликнули, а не
    /// только по итоговому значению enum.</summary>
    public readonly struct InventoryMainTabSelectedEvent : IEvent
    {
        public readonly InventoryGameplayMode Mode;
        public InventoryMainTabSelectedEvent(InventoryGameplayMode mode) => Mode = mode;
    }
    
    /// <summary>Клик по одной из доп. вкладок squad-режима (squadExtraBase/
    /// squadExtraTransport) — см. InventoryMainTabSelectedEvent докстринг про раздельные
    /// каналы при общем enum.</summary>
    public readonly struct InventorySquadExtraTabSelectedEvent : IEvent
    {
        public readonly InventoryGameplayMode Mode;
        public InventorySquadExtraTabSelectedEvent(InventoryGameplayMode mode) => Mode = mode;
    }

    
    
    /// <summary>
    /// Published whenever the inventory selection changes.
    ///
    /// CurrentItemId is null when the selection has been cleared.
    /// PreviousItemId is null when there was no previous selection.
    ///
    /// The event is a notification only. Consumers should query the current
    /// inventory state instead of using this payload as authoritative state.
    /// </summary>
    public readonly struct InventorySelectionChangedEvent : IEvent
    {
        public readonly IInventorySource Source;
        public readonly RuntimeId PreviousItemId;
        public readonly RuntimeId CurrentItemId;

        public bool HasSelection => CurrentItemId != null;

        public InventorySelectionChangedEvent(
            IInventorySource source,
            RuntimeId previousItemId,
            RuntimeId currentItemId)
        {
            Source = source;
            PreviousItemId = previousItemId;
            CurrentItemId = currentItemId;
        }
    }
    
    
    
    
    // ======= DRAG =============================================
    
    /// <summary>
    /// Describes why an inventory drag operation ended.
    /// </summary>
    public enum InventoryDragEndReason
    {
        Drop,
        Cancel,
        PointerReleased
    }
    
    /// <summary>
    /// Published once when an inventory drag operation starts.
    ///
    /// This is a lifecycle notification. The authoritative drag state
    /// remains owned by the inventory drag system and must be queried
    /// by consumers.
    /// </summary>
    public readonly struct InventoryDragStartedEvent : IEvent
    {
        public readonly IInventorySource Source;
        public readonly RuntimeId ItemId;
        public readonly int SlotIndex;

        public InventoryDragStartedEvent(
            IInventorySource source,
            RuntimeId itemId,
            int slotIndex)
        {
            Source = source;
            ItemId = itemId;
            SlotIndex = slotIndex;
        }
    }
    
    /// <summary>
    /// Published once when an inventory drag operation ends.
    ///
    /// The event must be published after the drag system has reset its
    /// authoritative drag state.
    /// </summary>
    public readonly struct InventoryDragEndedEvent : IEvent
    {
        public readonly IInventorySource Source;
        public readonly RuntimeId ItemId;
        public readonly int SlotIndex;
        public readonly InventoryDragEndReason Reason;

        public InventoryDragEndedEvent(
            IInventorySource source,
            RuntimeId itemId,
            int slotIndex,
            InventoryDragEndReason reason)
        {
            Source = source;
            ItemId = itemId;
            SlotIndex = slotIndex;
            Reason = reason;
        }
    }

    
    /// <summary>Предмет успешно перемещён между двумя ИНВЕНТАРНЫМИ ИСТОЧНИКАМИ (не между
    /// слотами одного источника — тот случай ItemTransferredEvent не поднимает, см. её
    /// докстринг в ITutorialInventoryTransferQuery). FromSourceType/ToSourceType — стабильные
    /// значения InventorySourceType, а не ссылки на IInventorySource: сами инстансы
    /// транзиентны (пересоздаются на каждый InventoryManagementController.BuildSources),
    /// поэтому identity здесь ровно как у ItemEquippedEvent.Slot — тип, не ссылка.</summary>
    public readonly struct ItemTransferredEvent : IEvent
    {
        public readonly InventorySourceType FromSourceType;
        public readonly InventorySourceType ToSourceType;
        public readonly InventorySlotRuntime Slot;

        public ItemTransferredEvent(
            InventorySourceType fromSourceType,
            InventorySourceType toSourceType, 
            InventorySlotRuntime slot)
        {
            FromSourceType = fromSourceType;
            ToSourceType = toSourceType;
            Slot = slot;
        }
    }
   

    
    
    public sealed class InventoryContentsChangedEvent : IEvent
    {
        public readonly IInventorySource Source;
        public InventoryContentsChangedEvent(IInventorySource source) => Source = source;
    }

    public sealed class ItemConsumabledEvent : IEvent
    {
        public readonly RuntimeId ItemId;
        public readonly int Amount;

        public ItemConsumabledEvent(RuntimeId itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    
    
    
    public sealed class ItemEquippedEvent : IEvent
    {
        public readonly EquipSlotType Slot;
        public readonly RuntimeId ItemId;

        public ItemEquippedEvent(
            EquipSlotType slot,
            RuntimeId itemId)
        {
            Slot = slot;
            ItemId = itemId;
        }
    }
    
    public readonly struct ItemUnequippedEvent : IEvent
    {
        public readonly EquipSlotType Slot;
        public readonly RuntimeId ItemId;

        public ItemUnequippedEvent(
            EquipSlotType slot,
            RuntimeId itemId)
        {
            Slot = slot;
            ItemId = itemId;
        }
    }
}