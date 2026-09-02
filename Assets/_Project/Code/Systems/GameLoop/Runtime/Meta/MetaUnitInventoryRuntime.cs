using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Инвентарь Meta-юнита (живой Proxy источник).
    /// </summary>
    public sealed class MetaUnitInventoryRuntime : IUnitInventoryRuntime
    {
        private readonly UnitRuntime _unit;

        public MetaUnitInventoryRuntime(UnitRuntime playerUnit) => _unit = playerUnit;

        public IInventorySource Equipment => _unit.Sources[0];
        public IInventorySource Cargo => _unit.Sources[1];
    }
}