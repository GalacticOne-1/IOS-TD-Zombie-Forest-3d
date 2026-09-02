
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    public interface IHasInventory
    {
        IReadOnlyDictionary<string, int> Resources { get; }
    }
}