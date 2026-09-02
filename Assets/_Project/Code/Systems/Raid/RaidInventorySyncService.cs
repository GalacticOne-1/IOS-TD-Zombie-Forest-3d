using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Raid
{
    public static class RaidInventorySyncService
    {
        
        public static void RestoreFromSnapshot(
            IInventorySource raidSource,
            IInventorySource proxySource)
        {
            var raidSlots = raidSource.GetSlots();
            var proxySlots = proxySource.GetSlots();

            int count = proxySlots.Count;

            for (int i = 0; i < count; i++)
            {
                if (i >= raidSlots.Count)
                {
                    proxySource.SetSlot(i, new InventorySlotRuntime(null, 0, 0, 0));
                    continue;
                }

                var raidSlot = raidSlots[i];

                if (raidSlot.IsEmpty)
                {
                    proxySource.SetSlot(i, new InventorySlotRuntime(null, 0, 0, 0));
                    continue;
                }

                proxySource.SetSlot(
                    i,
                    new InventorySlotRuntime(
                        raidSlot.Item,
                        raidSlot.Amount,
                        raidSlot.Durability,
                        raidSlot.AmmoInMagazine));
            }

            proxySource.NotifyChanged();
        }

    }
}