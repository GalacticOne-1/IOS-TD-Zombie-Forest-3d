using System.Collections.Generic;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Контекст проверки размещения.
    /// Содержит все данные для validator pipeline.
    /// </summary>
    public class PlacementValidationContext
    {
        public Vector2Int Origin;
        public BuildingFootprint Footprint;
        public int Rotation;

        public List<Vector2Int> Cells;

        public GridService Grid;
        public GridCoordinateService GridCoordinates;

        public FacilityModule Config;
    }
}