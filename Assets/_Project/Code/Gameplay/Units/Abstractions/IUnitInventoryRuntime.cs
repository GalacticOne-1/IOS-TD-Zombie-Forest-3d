using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Gameplay.Units.Abstractions
{
    /// <summary>
    /// Унифицированный runtime-доступ к инвентарю юнита.
    /// Позволяет боевым системам работать без знания слоя (Meta/Raid).
    /// </summary>
    public interface IUnitInventoryRuntime
    {
        IInventorySource Equipment { get; }
        IInventorySource Cargo { get; }
    }
}