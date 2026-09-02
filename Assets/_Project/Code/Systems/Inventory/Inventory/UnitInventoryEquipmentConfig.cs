using System.Collections.Generic;
using Galactic1.Core.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1.Code.Systems.Inventory
{
    [CreateAssetMenu(
        fileName = "UnitInventoryEquipmentConfig",
        menuName = "Game Configs/Inventory/Unit Equipment Config")]
    public class UnitInventoryEquipmentConfig : InventoryDataBase
    {
        
        public override void Initialize(Object data = null)
        {
            equipmentSlots = new Dictionary<int, EquipmentSlotType>()
            {
                { 0, EquipmentSlotType.WeaponMain },
                { 1, EquipmentSlotType.WeaponSecondary },
                
                { 2, EquipmentSlotType.QuickSlot1 },
                { 3, EquipmentSlotType.QuickSlot2 },
                { 4, EquipmentSlotType.QuickSlot3 },
                { 5, EquipmentSlotType.QuickSlot4 },
                
                { 6, EquipmentSlotType.Head },
                { 7, EquipmentSlotType.Body },
                { 8, EquipmentSlotType.Pants },
                { 9, EquipmentSlotType.Legs },
            };
        }
        
        
        
        public override EquipSlotType GetEquipmentSlotType(int slotIndex)
            => slotIndex switch
            {
                0 or 1 => EquipSlotType.Weapon,
                2 or 3 or 4 or 5 => EquipSlotType.Usable,
                6 => EquipSlotType.Head,
                7 => EquipSlotType.Torso,
                8 => EquipSlotType.Pants,
                9 => EquipSlotType.Boots,
            };
        

        
    }
}