
using Galactic1.Code.Gameplay.Grid;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Проверяет что клетки находятся внутри строительной сетки.
    /// </summary>
    public class GridBoundsRule : IPlacementRule
    {
        private readonly GridCoordinateService _coordinates;

        public GridBoundsRule(GridCoordinateService coordinates)
        {
            _coordinates = coordinates;
        }

        public PlacementValidationResult Validate(PlacementValidationContext context)
        {
            foreach (var c in context.Cells)
            {
                if (!_coordinates.IsInsideGrid(c))
                    return PlacementValidationResult.Fail(
                        "Outside construction zone",
                        context.Cells);
            }

            return null;
        }

    }
}