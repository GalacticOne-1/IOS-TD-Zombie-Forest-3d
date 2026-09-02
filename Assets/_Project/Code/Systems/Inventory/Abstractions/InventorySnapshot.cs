using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Inventory.Sources;

namespace Galactic1.Code.Inventory.Abstractions
{
    /// <summary>
    /// Полная копия инвентаря юнита на момент старта рейда.
    /// Все изменения во время боя происходят здесь.
    /// </summary>
    public sealed class InventorySnapshot
    {
        public List<InventorySlotRuntime> Slots = new();

        /// <summary>
        /// Создание snapshot из обычного proxy-инвентаря.
        /// </summary>
        public static InventorySnapshot CreateFromSource(
            IInventorySource source,
            InventoryAccessService access)
        {
            var snapshot = new InventorySnapshot();

            var proxySlots = access.GetSlots(source);
            foreach (var s in proxySlots)
            {
                snapshot.Slots.Add(new InventorySlotRuntime(
                    s.Item,
                    s.Amount,
                    s.Durability,
                    s.AmmoInMagazine));
            }

            return snapshot;
        }
        
        public static InventorySnapshot CreateFromLoot(
            List<InventorySlotRuntime> loot,
            int capacity)
        {
            var snapshot = new InventorySnapshot();

            foreach (var slot in loot)
                snapshot.Slots.Add(slot);

            while (snapshot.Slots.Count < capacity)
                snapshot.Slots.Add(new InventorySlotRuntime(null, 0, 0, 0));

            return snapshot;
        }
    }
}