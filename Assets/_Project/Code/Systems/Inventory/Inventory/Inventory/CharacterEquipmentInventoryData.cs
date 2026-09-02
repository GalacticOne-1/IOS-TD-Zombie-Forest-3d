
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Object = UnityEngine.Object;

namespace Galactic1.Code.UI.Inventory
{
    public abstract class CharacterEquipmentInventoryData : BaseInventoryData
    {
        public Dictionary<int, EquipmentSlotType> slotTypes = new();


        
        
        
        
        
        public override void Initialize(Object data = null)
        {
            var containerConfig = data as EquipmentContainerConfig;
            slotTypes = containerConfig.GetEquipmentSlotTypes();

            if (InventoryProxy.Slots.Count == 0)
            {
                foreach (var slotType in slotTypes.Values)
                    InventoryProxy.Slots
                        .Add(new InventorySlotProxy(new InventorySlotData(null, "", 0 , 0, 0)));
            }
        }


        
        
        
        public override int? FindSlotIndex(EquipmentSlotType requiresType)
        {
            foreach (var slot in slotTypes)
                if (slot.Value == requiresType)
                    return slot.Key;

            return null;
        }
        


        /// <summary>
        /// true - сумку можно удалить из экипировки
        /// </summary>
        /// <param name="inventory">Левая сторона, для провеикр удаляемых слотов</param>
        /// <param name="bagSlotIndex"></param>
        /// <returns></returns>
        public bool CanRemoveBag(CharacterInventoryData inventory, int bagSlotIndex)
        {
            // Получаем суммарное количество доступных слотов без этой сумки
            int totalExtra = 0;
            foreach (var kvp in slotTypes)
            {
                // считаем оставшиеся сумки
                if (kvp.Key != bagSlotIndex &&
                    (kvp.Value == EquipmentSlotType.Bag1 ||
                     kvp.Value == EquipmentSlotType.Bag2 ||
                     kvp.Value == EquipmentSlotType.Bag3 ||
                     kvp.Value == EquipmentSlotType.Bag4))
                {
                    var bagSlot = InventoryProxy.Slots[kvp.Key];
                    // if (bagSlot?.Item.Value?.Config is BagConfig bagConfig)
                    //     totalExtra += bagConfig.Capacity;
                }
            }

            int activeSlots = inventory.baseCapacity + totalExtra;

            // Проверяем все слоты, которые станут недоступными
            for (int i = activeSlots; i < inventory.InventoryProxy.Slots.Count; i++)
            {
                if (!inventory.InventoryProxy.Slots[i].IsEmpty)
                    return false; // есть предмет, который будет потерян
            }

            return true; // все слоты пустые
        }

        public abstract bool CanChangeBag(
            int targetSlotIndex,
            BaseInventoryData targetContainer,
            int slotIndex,
            InventorySlotRuntime oldSlot,
            InventorySlotRuntime incomingSlot,
            CharacterInventoryData inventory);

        public abstract void HandleBagChange(
            int slotIndex,
            InventorySlotRuntime oldSlot,
            InventorySlotRuntime incomingSlot,
            CharacterInventoryData inventory);

    }
}
