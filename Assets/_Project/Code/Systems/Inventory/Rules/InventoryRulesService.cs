using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Inventory.Rules
{
    
    /// <summary>
    /// Центральный геймплейный движок правил логистики.
    /// Решает МОЖНО ли переносить между источниками.
    /// </summary>
    public sealed class InventoryRulesService
    {

        public bool IsEquipmentSource(IInventorySource source)
            => source.Type == InventorySourceType.UnitEquipment ||
               source.Type == InventorySourceType.TransportEquipment;
        
        
        
        public bool CanMove(
            IInventorySource from,
            IInventorySource to,
            InventorySlotProxy slot)
        {
            if (from.IsReadOnly || to.IsReadOnly)
                return false;

            // 🚫 Нельзя перекладывать напрямую между экипировками юнитов
            if (from.Type == InventorySourceType.UnitEquipment &&
                to.Type == InventorySourceType.UnitEquipment)
                return false;

            // 🚫 Лут → только в транспорт
            // if (from.Type == InventorySourceType.LootContainer &&
            //     to.Type != InventorySourceType.VehicleCargo)
            //     return false;

            return true;
        }
    }
}