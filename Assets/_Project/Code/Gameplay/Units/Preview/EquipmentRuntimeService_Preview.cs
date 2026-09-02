using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment.Snapshots;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Meta.Stats;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// Геймплейная логика экипировки (НЕ MonoBehaviour).
    /// Работает поверх IInventorySource и не зависит от Proxy/Snapshot.
    ///
    /// Отвечает за:
    /// • определение экипированных предметов
    /// • модификаторы статов
    /// • урон прочности
    /// • авто-снятие при 0 durability
    /// • события экипировки
    /// </summary>
    public sealed class EquipmentRuntimeService_Preview : IEquipmentStatsProvider
    {
        public IInventorySource Source { get; private set; }

        public event Action<EquipSlotType, ItemConfig> OnEquipped;
        public event Action<EquipSlotType> OnUnequipped;
        public event Action OnClearAll;
        public event Action<ItemConfig> OnItemBroken;
        public event Action OnUpdate;
        
        
        public void BindSource(IInventorySource source)
        {
            Source = source;
        }
        
        
        
        // =========================================================
        // EQUIPPED STATE
        // =========================================================
        public void RestoreEquipmentFromInventory()
        {
            // удаляем старые модели
            OnClearAll?.Invoke();
            

            // создаём модели для каждого слота
            var slots = Source.GetSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                var item = slots[i].Item;
                if (item == null) continue;

                var slotType = Source.GetSlotType(i);

                // не спавним быстрые слоты
                if (slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2) continue;

                //BindVisual(item.Config.EquipSlotType, item);
                OnEquipped?.Invoke(item.GetEquipSlot(), item);
            }

        }

        public IReadOnlyList<ItemStatEntry> GetEquippedModifiers()
        {
            return null;
        }

        public EquipmentSnapshot CreateReadonlySnapshot()
        {
            return null;
        }

        public void ApplyDurabilityDamage(float damage) {}


        // =========================================================
        // EQUIP / UNEQUIP
        // =========================================================


        /// <summary> Надеть предмет, вернуть true если успешно </summary>
        public bool Equip(int slotIndex)
        {
            var slot = Source.GetSlot(slotIndex);
            var item = slot.Item;
            if (item == null) 
                return false;

            OnEquipped?.Invoke(item.GetEquipSlot(), item);

            return true;
        }


        /// <summary> Снять предмет из слота </summary>
        public void Unequip(int slotIndex)
        {
            var equipSlotType = Source.GetEquipmentSlotType(slotIndex);
            //Inventory.SetItem(slot, null);

            //ClearVisual(equipSlotType);
            OnUnequipped?.Invoke(equipSlotType);
        }

    }
}
