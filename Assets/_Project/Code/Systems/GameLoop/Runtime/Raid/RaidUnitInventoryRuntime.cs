using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Инвентарь юнита в рейде.
    /// Может быть ограничен, копией или временным.
    /// </summary>
    public sealed class RaidUnitInventoryRuntime : IUnitInventoryRuntime
    {
        public IInventorySource Equipment { get; }
        public IInventorySource Cargo { get; }

        public RaidUnitInventoryRuntime(
            IInventorySource equipment,
            IInventorySource cargo)
        {
            Equipment = equipment;
            Cargo = cargo;
        }
    }
}