using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
namespace Galactic1.Code.Gameplay.Equipment
{
    public static class EquipmentUtility
    {
        public static int? GetSlotType(IInventorySource source, EquipSlotType slot)
        {
            var type = slot switch
            {
                EquipSlotType.Weapon => EquipmentSlotType.WeaponMain,
                EquipSlotType.Head => EquipmentSlotType.Head,
                EquipSlotType.Torso => EquipmentSlotType.Body,
                _ => EquipmentSlotType.None
            };

            return source.FindSlotIndex(type);
        }
    }
}