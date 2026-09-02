
using Galactic1.Code.Cameras;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Управляет ghost объектом при строительстве
    /// </summary>
    public class ConstructionGhostController : MonoBehaviour
    {
        private ConstructionModeController _controller;
        private ConstructionService _constructionService;
        private CameraController _camera;
        
        
        private GameObject _ghost;
        private FacilityModule _itemConfig;
        private GhostShaderController _shader;
        


        public BuildableObject Ghost { get; private set; }
        public bool HasGhost => _ghost != null;
        
        

        public void Initialize(
            ConstructionModeController controller,
            ConstructionService constructionService,
            CameraController camera)
        {
            _controller = controller;
            _constructionService = constructionService;
            _camera = camera;
        }
        
        
        


        public void CreateGhost(FacilityModule config)
        {
            _itemConfig = config;

            _ghost = $"Prefabs/Gameplay/Entities/Facilities/{_itemConfig.Item.PrefabPath}_ghost"
                .CreateGO(ServiceLocator.Current.Get<Environment>().playerObj);

            Ghost = _ghost.GetComponent<BuildableObject>();
            Ghost.Facility.ItemConfig = _itemConfig.Item;
            
            _shader = Ghost.gameObject.AddComponent<GhostShaderController>();

            MoveToCameraCenter();
        }

        public void MoveTo(Vector2Int cell)
        {
            if (!HasGhost)
                return;
            
#if UNITY_EDITOR
            DLog.Alert($"MoveTo: {cell}", EDlogColor.YELLOW);
#endif

            Vector3 world = _constructionService
                .GetBuildingWorldPosition(cell, _controller.Context.Preview.Footprint);

            world.y = _ghost.transform.position.y;

            _ghost.transform.position = world;
        }
        
        public void SetRotation(int rotation)
        {
            if (!HasGhost)
                return;

            Ghost.transform.rotation = Quaternion.Euler(
                0,
                rotation * 90f,
                0);
        }
        

        
        
        public void DestroyGhost()
        {
            if (HasGhost)
            {
                Destroy(_ghost);
                _ghost = null;
            }
        }

        public Vector2Int CurrentCell()
        {
            if (!HasGhost)
                return Vector2Int.zero;

            Vector3 world = _ghost.transform.position;

            Vector2Int size = new Vector2Int(
                _itemConfig.FootprintConfig.width,
                _itemConfig.FootprintConfig.height);

            world.x -= size.x * _constructionService.Coordinates.CellSize / 2f;
            world.z -= size.y * _constructionService.Coordinates.CellSize / 2f;

            return _constructionService.Coordinates.WorldToCell(world);
        }

        void MoveToCameraCenter()
        {
            if (_camera.ScreenCenterToBuildPlane(out Vector3 world, new Vector2(0, 0)))
            {
                var coord = _constructionService.Coordinates.WorldToCell(world);
                
                MoveTo(coord);
            }
        }

        public void SetValid(bool valid)
        {
            if (_shader != null)
                _shader.SetValid(valid);
        }
    }
}