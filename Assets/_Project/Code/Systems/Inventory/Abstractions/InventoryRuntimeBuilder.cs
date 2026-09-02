
using System.Collections.Generic;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Items;

namespace Galactic1.Code.Inventory.Abstractions
{
    public static class InventoryRuntimeBuilder
    {
        /// <summary>
        /// Создаёт runtime слоты из Proxy (стратегический режим)
        /// </summary>
        public static List<InventorySlotRuntime> BuildFromProxy(
            InventoryProxy proxy,
            InventoryDataBase config)
        {
            var list = new List<InventorySlotRuntime>(proxy.Slots.Count);

            foreach (var p in proxy.Slots)
            {
                list.Add(new InventorySlotRuntime(
                    p.Item.Value, 
                    p.Amount.Value, 
                    p.Durability.Value,
                    p.AmmoInMagazine.Value));
            }
            
            return list;
        }
    }
}