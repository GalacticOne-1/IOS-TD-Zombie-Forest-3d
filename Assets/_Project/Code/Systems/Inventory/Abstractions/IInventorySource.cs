
using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Inventory.Abstractions
{
    /// <summary>
    /// Маркерный интерфейс источника инвентаря.
    /// Ничего не меняет в логике — используется для абстрагирования
    /// и перехода к контекстной системе доступа.
    /// </summary>
    public interface IInventorySource
    {
        string SourceId { get; }
        object Owner { get; } // КТО владелец слотов
        InventorySourceType Type { get; } 
        bool IsReadOnly { get; }
        
        
        /// UI / временные подписки
        event Action OnChanged;  
        /// Runtime / HUD / постоянные
        event Action OnChangedPersistent;
        
        InventoryDataBase InventoryData { get; }
        
        IEquipmentStateListener EquipmentListener { get; }
        
        
        // ================= SLOT API =================
        void Dispose();
        IReadOnlyList<InventorySlotRuntime> GetSlots();
        InventorySlotRuntime GetSlot(int index);
        void SetSlot(int index, InventorySlotRuntime slot);
        void ClearSlot(int index);
        
        
        bool HasOverflow(int newCapacity);
        List<InventorySlotRuntime> GetOverflowItems(int newCapacity);
        
        /// <summary>
        /// Возвращает общее количество предмета.
        /// </summary>
        int GetTotalAmount(RuntimeId itemId);
        /// <summary>
        /// Пытается добавить предмет в инвентарь.
        /// Возвращает false если нет места.
        /// </summary>
        AddItemResult TryAdd(InventorySlotRuntime slotRuntime);

        /// <summary>
        /// Проверка добавления одного слота
        /// <br/>Без мутации состояния
        /// </summary>
        bool CanAdd(InventorySlotRuntime slotRuntime);
        
        // ================= META =================
        void NotifyChanged();
        int? FindSlotIndex(EquipmentSlotType requiresType);
        EquipSlotType GetEquipmentSlotType(int fromIndex);
        EquipmentSlotType? GetSlotType(int slotIndex);
        IReadOnlyDictionary<int, EquipmentSlotType> EquipmentSlots { get; }
    }
}