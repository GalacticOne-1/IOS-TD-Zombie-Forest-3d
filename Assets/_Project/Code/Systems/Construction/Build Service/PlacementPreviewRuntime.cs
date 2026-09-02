
using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Runtime модель preview размещения здания.
    /// Хранит логическое состояние размещения независимо от Scene.
    /// </summary>
    public class PlacementPreviewRuntime
    {
        public FacilityModule Config;
        public BuildingFootprint Footprint;

        public Vector2Int Origin;
        public int Rotation;

        public bool IsValid;

        public List<Vector2Int> Cells;

        public void Initialize(FacilityModule config)
        {
            Config = config;
            Footprint = config.FootprintConfig.ToFootprint();
            Rotation = 0;
        }

        public void SetPlacement(Vector2Int origin, List<Vector2Int> cells, bool isValid)
        {
            Origin = origin;
            Cells = cells;
            IsValid = isValid;
        }

        public void Clear()
        {
            Config = null;
            Footprint = null;
            Cells = null;
            IsValid = false;
        }
    }
}