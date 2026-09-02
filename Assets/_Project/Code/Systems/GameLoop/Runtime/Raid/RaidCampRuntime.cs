using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-копия транспорта для рейда.
    /// Работает ТОЛЬКО с snapshot-инвентарём.
    /// </summary>
    public sealed partial class RaidCampRuntime
    {
        public string Id { get; }
        public RaidCampInventoryRuntime Sources { get; }

        
        
        public RaidCampRuntime(
            CampRuntime source,
            InventoryAccessService access)
        {
            Id = source.inventoryConfig.GetInventoryId(StorageType.Regular);

            Sources = new RaidCampInventoryRuntime(
                new RaidCampInventorySource(
                    source.inventoryConfig.GetInventoryId(StorageType.Regular),
                    source,
                    InventorySnapshot.CreateFromSource(source.GetInventory(StorageType.Regular), access),
                    source.GetInventory(StorageType.Regular).InventoryData,
                    null
                )
            );
        }

    }
    
}