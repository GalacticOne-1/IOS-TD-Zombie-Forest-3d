using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-копия транспорта для рейда.
    /// Работает ТОЛЬКО с snapshot-инвентарём.
    /// </summary>
    public sealed partial class RaidVehicleRuntime
    {
        public string Id { get; }
        public RaidVehicleInventoryRuntime Sources { get; }

        
        /* Никаких ссылок на VehicleRuntime после конструктора */
        
        public RaidVehicleRuntime(
            TransportRuntime source,
            InventoryAccessService access)
        {
            Id = source.Proxy.Id;

            Sources = new RaidVehicleInventoryRuntime(
                new RaidVehicleInventorySource(
                    source.Proxy.Id,
                    source,
                    InventorySnapshot.CreateFromSource(source.Sources[0], access),
                    source.Sources[0].InventoryData,
                    source
                ),
                new RaidVehicleInventorySource(
                    source.Proxy.Id,
                    source,
                    InventorySnapshot.CreateFromSource(source.Sources[1], access),
                    source.Sources[1].InventoryData,
                    source
                )
            );
        }

        private static IInventorySource CreateCargoInventory(
            TransportRuntime transport,
            InventoryAccessService access)
        {
            return new RaidVehicleInventorySource(
                transport.Proxy.Id,
                transport,
                InventorySnapshot.CreateFromSource(transport.Sources[1], access),
                transport.Sources[1].InventoryData,
                transport
            );
        }
    }
}