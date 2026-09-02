using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Raid
{
    public sealed class RaidCampInventoryRuntime : IUnitInventoryRuntime
    {
        /// Времменный инвентарь для локации
        public IInventorySource Equipment { get; }
        /// Времменный инвентарь для локации
        public IInventorySource Cargo { get; }

        public RaidCampInventoryRuntime(IInventorySource source)
        {
            Cargo = source;
        }
    }
}