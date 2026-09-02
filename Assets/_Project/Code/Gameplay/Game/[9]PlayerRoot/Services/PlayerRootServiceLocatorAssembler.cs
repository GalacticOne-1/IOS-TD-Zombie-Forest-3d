
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Combat.Visual;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.RaidLoot;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Gameplay.Player;
using Galactic1.RaidLoot.Scene;
using Galactic1.Repository;
using Galactic1.Systems.Inventory;
using UnityEngine;

namespace Galactic1
{
    public class PlayerRootServiceLocatorAssembler : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private CombatCameraShakeDriver _combatCameraShakeDriver;
        [SerializeField] private HubMaterials _hubMaterials;
        [SerializeField] private SquadController _squadController;
        [SerializeField] private LootRewardsWorldSystem _lootRewardsWorld;
        [SerializeField] private ContainerProgressWorldSystem _containerProgressWorld;
        
        
        
        public void Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            
            // register
            ServiceLocator.Current.Register(FindAnyObjectByType<SceneSessionManager>());
            ServiceLocator.Current.Register(_cameraController);
            ServiceLocator.Current.Register(new InventoryRepository());
            ServiceLocator.Current.Register(new PlayerRepository());
            ServiceLocator.Current.Register(_hubMaterials);
            
            
            ServiceLocator.Current.Register(_squadController);
            ServiceLocator.Current.Register(_lootRewardsWorld);
            ServiceLocator.Current.Register(_containerProgressWorld);
            
            
            // === camera shake
            var cameraShakeService = new CombatCameraShakeService(
                _cameraController.Camera, 
                _cameraController,
                configProvider.Get<CombatCameraShakeConfig>());
            var cameraShakeListener = new CombatCameraShakeListener(cameraShakeService);
            _combatCameraShakeDriver.Construct(cameraShakeService);
            ServiceLocator.Current.Register(_combatCameraShakeDriver);
            
            
            
            // ==== unregister
            var sceneServicesClear = new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<SceneSessionManager>();
                ServiceLocator.Current.Unregister<CameraController>();
                
                cameraShakeListener.Dispose();
                ServiceLocator.Current.Unregister<CombatCameraShakeDriver>();
                
                ServiceLocator.Current.Unregister<InventoryRepository>();
                ServiceLocator.Current.Unregister<PlayerRepository>();
                ServiceLocator.Current.Unregister<HubMaterials>();
                
                ServiceLocator.Current.Unregister<SquadController>();
                ServiceLocator.Current.Unregister<LootRewardsWorldSystem>();
                ServiceLocator.Current.Unregister<ContainerProgressWorldSystem>();
            });
            EventBus<SceneServicesClearEvent>.Register(sceneServicesClear);
        }
    }
}