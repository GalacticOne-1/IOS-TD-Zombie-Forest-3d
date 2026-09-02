using UnityEngine;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Items;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Dragon Equipment Data")]
    public class DragonEquipmentInventoryData : CharacterEquipmentInventoryData
    {
        
        public override EquipSlotType GetEquipmentSlotType(int slotIndex)
            => slotIndex switch
            {
                0 => EquipSlotType.Weapon,
                1 => EquipSlotType.Bag,
                5 => EquipSlotType.Shield,
            };

        public override void Initialize(Object data = null)
        {
            InventoryProxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[1].EquipmentProxy;
            
            base.Initialize(data);
            
            //slotTypes = _equipmentContainerConfig.GetEquipmentSlotTypes();
            
            // 
            // if (InventoryProxy.Slots.Count == 0)
            // {
            //     foreach (var slotType in slotTypes.Values)
            //         AddSlot(slotType);
            //     
            //     // AddSlot(EquipmentSlotType.WeaponMain);
            //     // AddSlot(EquipmentSlotType.Bag1);
            //     // AddSlot(EquipmentSlotType.QuickSlot1);
            //     // AddSlot(EquipmentSlotType.QuickSlot2);
            //     //
            //     // AddSlot(EquipmentSlotType.MagicWeapon);
            //     // AddSlot(EquipmentSlotType.Shield);
            // }

            // slotTypes = new()
            // {
            //     { 0, EquipmentSlotType.WeaponMain },
            //     { 1, EquipmentSlotType.Bag1 },
            //     { 2, EquipmentSlotType.QuickSlot1 },
            //     { 3, EquipmentSlotType.QuickSlot2 },
            //     
            //     { 4, EquipmentSlotType.MagicWeapon },
            //     { 5, EquipmentSlotType.Shield },
            // };
        }
        
        public override EquipmentSlotType GetSlotType(int slotIndex) => slotTypes[slotIndex];

        // private void AddSlot(EquipmentSlotType type)
        // {
        //     // int index = slots.Count;
        //     InventoryProxy.Slots.Add(new InventorySlotProxy(new InventorySlotData(null, "", 0 , 0)));
        //     // slotTypes[index] = type;
        // }

       





        #region BAGS


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
        /// <param name="playerInventory">Инвентарь игрока</param>
        /// <returns>true если операция безопасна</returns>
        public override bool CanChangeBag(
            int targetSlotIndex,
            BaseInventoryData targetContainer,
            int slotIndex,
            InventorySlotRuntime oldSlot,
            InventorySlotRuntime incomingSlot,
            CharacterInventoryData inventory)
        {
            

            return true;
        }

        #endregion
    }
}
