using Galactic1.Items;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Inventory.Services
{
    public sealed class EquipmentValidationService
    {
        
        /// <summary>
        /// Проверяет источник инвентаря для предмета
        /// <br/>unit/vehicle
        /// </summary>
        /// <param name="targetSource"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CheckSource(
            IInventorySource targetSource,
            ItemConfig item)
        {
            if (item == null) return false;

            var owner = targetSource.Owner;

            // 🧍 Юнит
            if (owner is IUnitRuntime)
                return !item.HasModule<VehicleEquipmentModule>();

            // 🚗 Техника
            if (owner is TransportRuntime)
                return item.HasModule<VehicleEquipmentModule>();

            return false;
        }
        
        /// <summary>
        /// Проверяет предмет и тип слота (для снаряги)
        /// </summary>
        /// <param name="targetSource"></param>
        /// <param name="slotType"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanEquip(
            IInventorySource targetSource,
            EquipmentSlotType slotType,
            ItemConfig item)
        {
            if (item == null) return false;

            var owner = targetSource.Owner;

            // 🧍 Юнит
            if (owner is IUnitRuntime)
                return ValidateForUnit(slotType, item);

            // 🚗 Техника
            if (owner is TransportRuntime vehicle)
                return ValidateForVehicle(slotType, item);

            return false;
        }
        
        

        private bool ValidateForUnit(EquipmentSlotType slot, ItemConfig item)
        {
            if (!InventoryRules.IsEquipTypeAllowedForSlot(item.GetEquipSlot(), slot))
                return false;

            // if (!unit.Config.AllowedWeaponTypes.Contains(item.Config.WeaponType))
            //     return false;

            if (item.HasModule<VehicleEquipmentModule>())
                return false;

            return true;
        }

        private bool ValidateForVehicle(EquipmentSlotType slot, ItemConfig item)
        {
            // if (!vehicle.Config.AllowedModuleSlots.Contains(slot))
            //     return false;
            //
            // if (!vehicle.Config.AllowedItemCategories.Contains(item.Config.Category))
            //     return false;
            
            if (!item.HasModule<VehicleEquipmentModule>())
                return false;

            return true;
        }
    }
}