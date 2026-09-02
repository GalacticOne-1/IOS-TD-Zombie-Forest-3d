using Galactic1.Code.Cameras;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.WorldMap;
using Galactic1.Configs;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.GameSession.WorldMap;
using Galactic1.Core.UI;
using UnityEngine;

namespace Galactic1.Core.Systems.GameSession
{
    public class MapSceneSessionManager : SceneSessionManager
    {
        
        
        protected override void Initialize(DIContainer container)
        {
            
            var gameStateProvider = _container.Resolve<IGameStateProvider>();
            var gameStateProxy = gameStateProvider.GameStateProxy;
            var configProvider = _container.Resolve<IConfigProvider>();
            
            
            // 1. Создаём session
            _session = new SceneSessionDefinition();
            
            // 2. Создаём контекст карты
            var mapContext = new WorldMapContext
            {
                MapConfig = configProvider.Get<LocationsConfigs>().WorldMapConfig,
                CurrentDay = gameStateProxy.GameLoopContext.CurrentDay.Value,
                //TransportState = repo.TransportState,
                //HordeState = repo.HordeState,
                //CurrentNode = repo.CurrentMapNode
            };
            
            _session.InventoryPort = container.Resolve<IInventoryResourcesPort>();
            
            _session.WorldMapContext = mapContext;
            
            // 3. Загружаем карту
            new WorldMapLoader().Load(ref _session);
            
            
            // 4. Инициализация систем
            InitializeLevelSystems();
            
            
            // *****************************************************************************************************
            // *** clearing
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                
            }));
        }



        
        
        protected override void InitializeLevelSystems()
        {
            // Включаем камеру
            ServiceLocator.Current.Get<IMainCamera>().OnLevelLoaded(
                _container.Resolve<IConfigProvider>().Get<CameraConfigs>().WorldmapCamera,
                _session.CameraPosition,
                _session.WorldMapContext.MapConfig.CameraMinBounds,
                _session.WorldMapContext.MapConfig.CameraMaxBounds,
                6);
            
            // Для клика в сцене
            ServiceLocator.Current.Get<WorldInputDispatcher>()
                .Setup(ServiceLocator.Current.Get<IMainCamera>().Camera);
            
            // HUD
            //_session.HUD = FindAnyObjectByType<HUDPlayer>();
            

            Debug.Log("[GameSession] Level systems initialized");
        }
    }
}