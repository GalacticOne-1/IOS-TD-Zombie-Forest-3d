using System.Collections.Generic;
using Galactic1.Core.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1.Code.Systems.Inventory
{
    [CreateAssetMenu(
        fileName = "TransportInventoryEquipmentConfig",
        menuName = "Game Configs/Inventory/Transport Equipment Config")]
    public class TransportInventoryEquipmentConfig : InventoryDataBase
    {
        public override void Initialize(Object data = null)
        {
            equipmentSlots = new Dictionary<int, EquipmentSlotType>()
            {
                { 0, EquipmentSlotType.WeaponMain },
                { 1, EquipmentSlotType.QuickSlot1 },
                { 2, EquipmentSlotType.QuickSlot2 },
            };
        }
        
        
        
        public override EquipSlotType GetEquipmentSlotType(int slotIndex)
            => slotIndex switch
            {
                0 => EquipSlotType.Weapon,
                1 => EquipSlotType.Usable,
                2 => EquipSlotType.Usable,
            };
    }
}