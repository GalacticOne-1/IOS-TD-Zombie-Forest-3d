
using Galactic1.Code.Cameras;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Squad;
using Galactic1.Configs;
using Galactic1.Core.UI.HUD;
using Galactic1.Gameplay.Death;
using Galactic1.Gameplay.Locations;
using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.Gameplay.Locations.Navigation;
using UnityEngine;


namespace Galactic1.Core.Systems.GameSession
{
    public class DevSceneSessionManager : SceneSessionManager
    {
        
        
        protected override void Initialize(DIContainer container)
        {
            // #1 
            var gameLoopContext = gameSession.GameLoopContext;
            
            // #2 Загружаем локацию
            var locationLoader = FindAnyObjectByType<LocationLoader>();
            if (locationLoader != null)
                locationLoader.Load(out _session, _container);
            
            // #3 устанавливаем размер сетки
            var geometry = FindAnyObjectByType<LocationGeometryDefinition>();
            new LocationNavigationSystem().Configure(geometry.GetDto());
            
            _session.InventoryPort = gameLoopContext.CurrentRaid.PlayerTransport.Sources.Cargo as IInventoryResourcesPort;
            
            
            // #3 подключаем статы к прокси игрока
            //new UIStatsController().Register(container);
            
            // ******************************************************************************************************
            // ******************************************************************************************************
            
            
            // Далее спавним плеера, дракона, HUD...
            
            
            //SpawnDragon();
            SpawnPlayer(gameLoopContext);
            InitializeLevelSystems();
            
            // Создание врагов, сундуков и т.д.
            

            //EnemyFactory.CreateAll(context);
            //LootFactory.CreateAll(context);
            //ContainerFactory.CreateAll(context);

            
            // Когда ВСЁ ГОТОВО → активируем игрока
            //ActivateDragon();
            ActivatePlayer();
            
            
            // полный ресакн всех графов
            // обязательная пауза перед ресканом!!!!
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.5f, () =>
            {
                AstarPath.active.Scan();
            });
        }


        private void SpawnPlayer(GameLoopContext gameLoopContext)
        {
            var configProvider = _container.Resolve<IConfigProvider>();

            // === создание представления в сцене
            _container.Resolve<TransportSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Raid);
            _container.Resolve<UnitSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Raid);
            _container.Resolve<UnitSceneLifecycleSystem>().ActivateScene();
            
            
            // === scene runtime
            var squadScene = _container.Resolve<SquadSceneRuntime>();
    
            foreach (var survivor in _session.Survivors)
                squadScene.AddAgent(survivor);
            //
            
            
            
            // *****************************************************************************************************
            // *** clearing
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                //survivalIcons.gameObject.DestroyGO();
            }));
        }

        


        
        
        private void ActivatePlayer()
        {
            
            //new ParallaxInit(ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[0].general.BgSetup);

            // 2) Включаем управление
            //HUBController.I.INIT(); //player.EnableInput();
            
            // 3) Активируем UI
            //context.UIController.BindToPlayer(player);
            
            // Инициализация
            //_session.Player.Entity_Activate();
            
            
            // ! устанавливаем контроллер и аниматор после инициализации !
            //ControllableSwitcher.Restore(); //cmd.RestoreController();

            DLog.Alert("[GameSession] Player activated and ready.", AppConstants.show_log_core);
        }

        
        
        protected override void InitializeLevelSystems()
        {
            // Включаем камеру
            ServiceLocator.Current.Get<CameraController>().OnLevelLoaded(
                _container.Resolve<IConfigProvider>().Get<CameraConfigs>().LocationCamera,
                _session.LocationContext.CameraPosition,
                _session.LocationContext.CameraMinBounds,
                _session.LocationContext.CameraMaxBounds,
                2);
            
            // Для клика в сцене
            ServiceLocator.Current.Get<WorldInputDispatcher>()
                .Setup(ServiceLocator.Current.Get<CameraController>().Camera);
            
            // HUD
            _session.HUD = FindAnyObjectByType<HUDPlayer>();
            
            // player death service
            ServiceLocator.Current.Get<DeathSystem>().Initialize(_session);
            
           

            Debug.Log("[GameSession] Level systems initialized");
        }
    }
}