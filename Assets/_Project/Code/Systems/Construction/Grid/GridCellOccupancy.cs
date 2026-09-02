using Galactic1.Code.Gameplay.Construction;

namespace Galactic1.Code.Gameplay.Grid
{
    public struct GridCellOccupancy
    {
        public BuildableObject Object;
        public GridOccupancyType Type;

        public GridCellOccupancy(BuildableObject obj, GridOccupancyType type)
        {
            Object = obj;
            Type = type;
        }
    }
}