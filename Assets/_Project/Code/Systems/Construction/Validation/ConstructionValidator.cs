using System.Collections.Generic;
using UnityEngine;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Pipeline validator строительства.
    /// Прогоняет список правил.
    /// </summary>
    public class ConstructionValidator
    {
        private readonly GridService _grid;
        private readonly GridCoordinateService _coordinates;
        private readonly List<IPlacementRule> _rules;

        public ConstructionValidator(
            GridService grid,
            GridCoordinateService coordinates,
            GridBlockedAreaService blockedAreas)
        {
            _grid = grid;
            _coordinates = coordinates;

            _rules = new List<IPlacementRule>
            {
                new GridBoundsRule(coordinates),
                new StaticBlockedRule(blockedAreas),
                new OccupancyRule(grid)
            };
        }

        public PlacementValidationResult Validate(
            Vector2Int origin,
            BuildingFootprint footprint,
            int rotation,
            FacilityModule config = null)
        {
            var (w, h) = FootprintRotation.Rotate(
                footprint.Width, footprint.Height, rotation);

            var cells = _coordinates.GetFootprintCells(origin, w, h);

            var context = new PlacementValidationContext
            {
                Origin = origin,
                Cells = cells,
                Footprint = footprint,
                Rotation = rotation,
                Grid = _grid,
                GridCoordinates = _coordinates,
                Config = config
            };

            foreach (var rule in _rules)
            {
                var result = rule.Validate(context);
                if (result != null) return result;
            }

            return PlacementValidationResult.Pass(cells);
        }
    }
}