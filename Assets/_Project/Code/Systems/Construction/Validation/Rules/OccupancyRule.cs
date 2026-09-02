
using Galactic1.Code.Gameplay.Grid;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Проверяет заняты ли клетки зданиями.
    /// </summary>
    public class OccupancyRule : IPlacementRule
    {
        private readonly GridService _grid;

        public OccupancyRule(GridService grid)
        {
            _grid = grid;
        }

        public PlacementValidationResult Validate(PlacementValidationContext context)
        {
            if (!_grid.IsAreaFree(context.Cells))
            {
                return PlacementValidationResult.Fail(
                    "Something is blocking",
                    context.Cells);
            }

            return null;
        }
    }
}