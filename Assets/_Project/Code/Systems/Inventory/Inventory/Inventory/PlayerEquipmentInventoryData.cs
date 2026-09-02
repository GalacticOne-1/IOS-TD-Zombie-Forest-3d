using UnityEngine;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Player Equipment Data")]
    public class PlayerEquipmentInventoryData : CharacterEquipmentInventoryData
    {
        public override EquipSlotType GetEquipmentSlotType(int slotIndex)
            => slotIndex switch
            {
                0 => EquipSlotType.Weapon,
                1 => EquipSlotType.Bag,
                4 => EquipSlotType.Head,
                5 => EquipSlotType.Torso,
                6 => EquipSlotType.Pants,
                7 => EquipSlotType.Boots,
            };
        
        

        public override void Initialize(Object data = null)
        {
            InventoryProxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0].EquipmentProxy;
            
            base.Initialize(data);

            // var containerConfig = data as EquipmentContainerConfig;
            // slotTypes = containerConfig.GetEquipmentSlotTypes();
            
            // 
            // if (InventoryProxy.Slots.Count == 0)
            // {
            //     foreach (var slotType in slotTypes.Values)
            //         AddSlot(slotType);
            //     
                // AddSlot(EquipmentSlotType.WeaponMain);
                // AddSlot(EquipmentSlotType.Bag1);
                // AddSlot(EquipmentSlotType.QuickSlot1);
                // AddSlot(EquipmentSlotType.QuickSlot2);
                //
                // AddSlot(EquipmentSlotType.Head);
                // AddSlot(EquipmentSlotType.Body);
                // AddSlot(EquipmentSlotType.Pants);
                // AddSlot(EquipmentSlotType.Legs);
                
            //}

            // slotTypes = new()
            // {
            //     { 0, EquipmentSlotType.WeaponMain },
            //     { 1, EquipmentSlotType.Bag1 },
            //     { 2, EquipmentSlotType.QuickSlot1 },
            //     { 3, EquipmentSlotType.QuickSlot2 },
            //     
            //     { 4, EquipmentSlotType.Head },
            //     { 5, EquipmentSlotType.Body },
            //     { 6, EquipmentSlotType.Pants },
            //     { 7, EquipmentSlotType.Legs },
            //     
            // };
        }
        
        public override EquipmentSlotType GetSlotType(int slotIndex) => slotTypes[slotIndex];

        // private void AddSlot(EquipmentSlotType type)
        // {
        //     //int index = InventoryProxy.Slots.Count;
        //     InventoryProxy.Slots.Add(new InventorySlotProxy(new InventorySlotData(null, "", 0 , 0)));
        //     //slotTypes[index] = type;
        // }

        public override AddItemResult TryAdd(ItemConfig item, int amount)
        {
            // В экипировку можно положить только 1 предмет на слот
            for (int i = 0; i < InventoryProxy.Slots.Count; i++)
            {
                var slot = InventoryProxy.Slots[i];
                if (slot.IsEmpty && IsItemAllowedForSlot(item, slotTypes[i]))
                {
                    InventoryProxy.Slots[i] =
                        new InventorySlotProxy(
                            new InventorySlotData(item, "", 1, item.Physical.maxDurability, 0));
                    OnChanged?.Invoke();
                    return new AddItemResult();
                }
            }

            return new AddItemResult();
        }

        private bool IsItemAllowedForSlot(ItemConfig item, EquipmentSlotType slotType)
        {
            if (item == null) return false;

            switch (slotType)
            {
                case EquipmentSlotType.WeaponMain:
                //case EquipmentSlotType.WeaponSecondary:
                    return item.GetEquipSlot() == EquipSlotType.Weapon;
                case EquipmentSlotType.QuickSlot1:
                case EquipmentSlotType.Bag1:
                case EquipmentSlotType.Bag2:
                    return item.GetEquipSlot() == EquipSlotType.Bag;
                case EquipmentSlotType.Head:
                    return item.GetEquipSlot() == EquipSlotType.Head;
                case EquipmentSlotType.Body:
                    return item.GetEquipSlot() == EquipSlotType.Torso;
                case EquipmentSlotType.Pants:
                    return item.GetEquipSlot() == EquipSlotType.Pants;
                case EquipmentSlotType.Legs:
                    return item.GetEquipSlot() == EquipSlotType.Boots;
            }

            return false;
        }




        #region BAGS


        // public void HandleBagChange(int slotIndex, InventorySlot oldSlot, InventorySlot incomingSlot, InventoryData playerInventory)
        // {
        //     if (playerInventory == null) return;
        //
        //     if (!slotTypes.TryGetValue(slotIndex, out var slotType)) return;
        //     if (slotType != EquipmentSlotType.Bag1 && slotType != EquipmentSlotType.Bag2) return;
        //
        //     // 🔹 Локально обновляем слот, чтобы пересчёт включал текущее изменение
        //     var tempSlots = new List<InventorySlot>(slots);
        //     tempSlots[slotIndex] = incomingSlot;
        //
        //     // 🔹 Пересчитываем общий бонус со всех сумок
        //     int totalExtra = 0;
        //     foreach (var kvp in slotTypes)
        //     {
        //         if (kvp.Value == EquipmentSlotType.Bag1 || kvp.Value == EquipmentSlotType.Bag2)
        //         {
        //             var bagSlot = tempSlots[kvp.Key];
        //             if (bagSlot?.item?.config is BagConfig bagConfig)
        //                 totalExtra += bagConfig.backpackExtraSlots;
        //         }
        //     }
        //
        //     // 🔹 Применяем новое значение к инвентарю игрока
        //     playerInventory.SetBackpack(totalExtra);
        //
        //     // 🔹 Проверяем предметы, которые могут оказаться за пределами нового лимита
        //     int totalSlots = playerInventory.basePocketSlots + totalExtra;
        //     for (int i = totalSlots; i < playerInventory.slots.Count; i++)
        //     {
        //         var slot = playerInventory.slots[i];
        //         if (!slot.IsEmpty)
        //         {
        //             Debug.LogWarning($"⚠️ {slot.item.name} находится за пределами активных слотов (индекс {i}).");
        //             // slot.Clear(); // Включи, если нужно удалять предметы
        //         }
        //     }
        //
        //     playerInventory.OnChanged?.Invoke();
        // }



        public override void HandleBagChange(
            int slotIndex,
            InventorySlotRuntime oldSlot,
            InventorySlotRuntime incomingSlot,
            CharacterInventoryData inventory)
        {
            
        }


        /// <summary>
        /// Проверяет, можно ли безопасно удалить или заменить сумку,
        /// т.е. все слоты, которые станут неактивными, пустые.
        /// </summary>
        /// <param name="slotIndex">Индекс слота сумки (Bag1 или Bag2)</param>
        /// <param name="oldSlot">Текущая сумка в слоте</param>
        /// <param name="incomingSlot">Сумка, которая заменяет старую (null если удаляем)</param>
        /// <param name="inventory">Инвентарь игрока</param>
        /// <returns>true если операция безопасна</returns>
        public override bool CanChangeBag(
            int targetSlotIndex,
            BaseInventoryData targetContainer,
            int slotIndex,
            InventorySlotRuntime oldSlot,
            InventorySlotRuntime incomingSlot,
            CharacterInventoryData inventory)
        {
            return false;
        }

        // public override bool CanRemoveBag(CharacterInventoryData inventory, int bagSlotIndex)
        // {
        //     // Получаем суммарное количество доступных слотов без этой сумки
        //     int totalExtra = 0;
        //     foreach (var kvp in slotTypes)
        //     {
        //         if ((kvp.Value == EquipmentSlotType.Bag1 || kvp.Value == EquipmentSlotType.Bag2) && kvp.Key != bagSlotIndex)
        //         {
        //             var bagSlot = slots[kvp.Key];
        //             if (bagSlot?.item?.config is BagConfig bagConfig)
        //                 totalExtra += bagConfig.capacity;
        //         }
        //     }
        //
        //     int activeSlots = inventory.baseCapacity + totalExtra;
        //
        //     // Проверяем все слоты, которые станут недоступными
        //     for (int i = activeSlots; i < inventory.slots.Count; i++)
        //     {
        //         if (!inventory.slots[i].IsEmpty)
        //             return false; // есть предмет, который будет потерян
        //     }
        //
        //     return true; // все слоты пустые
        // }


        #endregion
    }
}
