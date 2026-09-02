
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    public interface IProductionBuilding
    {
        IReadOnlyList<string> AvailableRecipes { get; }
        bool IsProducing { get; }
    }
}