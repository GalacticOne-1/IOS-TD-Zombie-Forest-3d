using UnityEngine;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Основной сервис системы строительства.
    ///
    /// Роль:
    /// - управление размещением
    /// - перемещение объектов
    /// - удаление
    /// - регистрация occupancy сетки
    /// </summary>
    public class ConstructionService
    {
        public readonly GridService Grid;
        public readonly GridCoordinateService Coordinates;
        public readonly GridBlockedAreaService BlockedAreas;
        private readonly ConstructionValidator _validator;
        
        
        

        public ConstructionService(
            GridService grid,
            GridCoordinateService coordinates,
            GridBlockedAreaService blockedAreas)
        {
            Grid = grid;
            Coordinates = coordinates;
            BlockedAreas = blockedAreas;

            _validator = new ConstructionValidator(
                    grid,
                    coordinates,
                    blockedAreas);
        }

        public PlacementValidationResult ValidatePlacement(
            Vector2Int origin,
            BuildingFootprint footprint,
            int rotation,
            FacilityModule config = null)
        {
            return _validator.Validate(
                origin,
                footprint,
                rotation,
                config);
        }

        public void Register(BuildableObject obj)
        {
            Grid.Register(obj, obj.FootprintRuntime, GridOccupancyType.Facility);
        }

        public void Unregister(BuildableObject obj)
        {
            Grid.Unregister(obj, obj.FootprintRuntime);
        }

        public bool Move(BuildableObject obj, Vector2Int cell)
        {
            Grid.Unregister(obj, obj.FootprintRuntime);

            var result =
                _validator.Validate(
                    cell,
                    obj.FootprintRuntime.Footprint,
                    obj.FootprintRuntime.Rotation);

            if (!result.IsValid)
            {
                Grid.Register(obj, obj.FootprintRuntime, GridOccupancyType.Facility);
                return false;
            }

            obj.FootprintRuntime.Move(cell);

            Grid.Register(obj, obj.FootprintRuntime, GridOccupancyType.Facility);

            return true;
        }
        
        
        public Vector3 GetBuildingWorldPosition(
            Vector2Int origin,
            BuildingFootprint footprint)
        {
            return Coordinates.GetFootprintCenter(
                origin,
                footprint.Width,
                footprint.Height);
        }

    }
}