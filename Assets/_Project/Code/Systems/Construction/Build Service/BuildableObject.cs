
using System;
using Galactic1.AbstractFactory;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Scene адаптер построенного здания.
    ///
    /// Связывает:
    /// Runtime footprint
    /// Scene GameObject
    /// Grid регистрацию
    /// </summary>
    public class BuildableObject : MonoBehaviour
    {
        private Vector2Int objectSize;
        private bool snapToGrid = true;
        private Vector2 offsetOnGrid = Vector2.zero;
        
        
        private bool isInitialized = false;



        public FacilityInstance Facility 
            => GetComponent<FacilityInstance>();
        public FacilityModule FacilityConfig
            => GetComponent<_Entity>().ItemConfig.GetFacilityModule();

        private ConstructionService _constructionService;
        public ISceneFacility Adapter => Facility.FacilityAdapter;
        public BuildingFootprintRuntime FootprintRuntime { get; private set; }

        



        public void Bind(
            BuildingFootprintRuntime footprintRuntime,
            ConstructionService constructionService)
        {
            if (isInitialized) return;

            transform.position = Vector3.zero;
            _constructionService = constructionService;
            
            FootprintRuntime = footprintRuntime;
            Adapter.OnPositionChanged += OnRuntimePositionChanged;
            Adapter.OnRotationChanged += OnRuntimeRotationChanged;

            OnRuntimePositionChanged(Adapter.Position);
            OnRuntimeRotationChanged(Adapter.Rotation);
            

            objectSize = new Vector2Int(
                FootprintRuntime.Footprint.Width,
                FootprintRuntime.Footprint.Height);

            isInitialized = true;
        }

        private void OnDestroy()
        {
            if (Adapter != null)
                Adapter.Dispose();
        }

        private void OnRuntimePositionChanged(Vector2Int cell)
        {
            var rotatedFootprint = FootprintRuntime.Footprint.Rotate(FootprintRuntime.Rotation);
            Vector3 world = _constructionService.GetBuildingWorldPosition(cell, rotatedFootprint);
            world.y = transform.position.y;

            transform.position = world;
            FootprintRuntime.Move(cell);
        }
        
        public void OnRuntimeRotationChanged(int rotation)
        {
            transform.rotation = Quaternion.Euler(
                0,
                rotation * 90f,
                0);
            FootprintRuntime.Rotate(rotation);
        }
        
        
        void OnDrawGizmos()
        {
            // Visualize object size in editor
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + new Vector3(objectSize.x * 0.5f,  0, objectSize.y * 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(objectSize.x, objectSize.y, 1));
        }
        
        
    }
}
