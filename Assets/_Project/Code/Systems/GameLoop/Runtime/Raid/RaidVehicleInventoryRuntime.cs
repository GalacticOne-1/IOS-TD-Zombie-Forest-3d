using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Инвентарь юнита в рейде.
    /// Может быть ограничен, копией или временным.
    /// </summary>
    public sealed class RaidVehicleInventoryRuntime : IUnitInventoryRuntime
    {
        /// Времменный инвентарь для локации
        public IInventorySource Equipment { get; }
        /// Времменный инвентарь для локации
        public IInventorySource Cargo { get; }

        public RaidVehicleInventoryRuntime(
            IInventorySource equipment,
            IInventorySource cargo)
        {
            Equipment = equipment;
            Cargo = cargo;
        }
    }
}