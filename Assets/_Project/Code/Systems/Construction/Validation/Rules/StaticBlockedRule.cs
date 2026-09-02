using Galactic1.Code.Gameplay.Grid;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Проверяет пересечение клеток со статическими заблокированными зонами.
    /// Если зона заблокирована, но её тег разрешён для данного FacilityModule
    /// (context.Config), проверка пропускается.
    /// </summary>
    public class StaticBlockedRule : IPlacementRule
    {
        private readonly GridBlockedAreaService _blockedAreas;

        public StaticBlockedRule(GridBlockedAreaService blockedAreas)
        {
            _blockedAreas = blockedAreas;
        }

        public PlacementValidationResult Validate(PlacementValidationContext context)
        {
            foreach (var cell in context.Cells)
            {
                // if (!_blockedAreas.TryGetTag(cell, out var tag))
                //     continue;
                _blockedAreas.TryGetTag(cell, out var tag);

                bool allowed = context.Config != null && context.Config.IsZoneAllowed(tag);
                if (!allowed)
                {
                    return PlacementValidationResult.Fail(
                        "Area is blocked",
                        context.Cells);
                }
            }

            return null;
        }
    }
}