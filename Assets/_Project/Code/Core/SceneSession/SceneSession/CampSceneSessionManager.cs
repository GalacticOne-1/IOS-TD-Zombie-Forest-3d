
using Galactic1.Code.Cameras;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Configs;
using Galactic1.Core.UI;
using Galactic1.Core.UI.HUD;
using Galactic1.Gameplay.Death;
using Galactic1.Gameplay.Locations;
using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.Gameplay.Locations.Navigation;
using UnityEngine;

namespace Galactic1.Core.Systems.GameSession
{
    public class CampSceneSessionManager : SceneSessionManager
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

            _session.InventoryPort = container.Resolve<IInventoryResourcesPort>();
            
            
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
            
            // полный рескaн всех графов
            // обязательная пауза перед ресканом!!!!
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.5f, () =>
            {
                AstarPath.active.Scan();
            });
        }


        private void SpawnPlayer(GameLoopContext gameLoopContext)
        {
            var configProvider = _container.Resolve<IConfigProvider>();

            //var cameraTargetGroup = ServiceLocator.Current.Get<CameraTargetGroup>();
            //cameraTargetGroup.Clear();

            //var playerFactory = ServiceLocator.Current.Get<PlayerFactory>();

            // === спавним всех юнитов игрока
            //var unitRuntime = gameLoopContext.PlayerUnits.ToList();
            //var l = unitRuntime.Count;
            //_session.Survivors = new(l);
            // for (int i = 0; i < l; i++)
            // {
            //     var survivor = playerFactory.Create(i, _session, unitRuntime[i]);
            //     
            //     // сервис фокуса камеры на отряде
            //     cameraTargetGroup.Add(survivor.tr);
            //     survivor.OnDeath += () => cameraTargetGroup.Remove(survivor.tr);
            //     survivor.OnDestory += () => cameraTargetGroup.Remove(survivor.tr);
            // }
            
            // === создание представления в сцене
            _container.Resolve<TransportSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Camp);
            _container.Resolve<UnitSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Camp);
            

            
            
            
            // создаем иконки голода и жажды
            // var survivalIcons = "Prefabs/UI/Gameplay/HUDSurvival"
            //     .CreateGO(ServiceLocator.Current.Get<UIManager>().TransformRoot.hudRoot)
            //     .GetComponent<SurvivalIcons>();
            // survivalIcons.Initialize(_session.Player.GetComponent<SurvivalController>(), new Vector2(0, 1.6f));
            
            
            
            
            
            // * активация контроллера движения
            // switch (configProvider.Get<ApplicationConfig>().playerControllerType)
            // {
            //     case ApplicationConfig.EPlayerController.MOBILE:
            //         //JoystickController.I.Activator();
            //         //EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(JoystickController.I.IUpdateClear));
            //         break;
            //
            //     case ApplicationConfig.EPlayerController.KEYBOARD:
            //         //Player2dController.I.Activator();
            //         //EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(Player2dController.I.IUpdateClear));
            //         break;
            // }
            
            
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
                _container.Resolve<IConfigProvider>().Get<CameraConfigs>().CampCamera,
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
            
            // player FSM
            //var playerStateMachine = _session.Player.GetComponent<PlayerStateMachine>();
            //ServiceLocator.Current.Get<WorldInputDispatcher>().Setup(playerStateMachine);
            //JoystickController.I.Setup(playerStateMachine);
            
            // player interaction system
            //ServiceLocator.Current.Get<DragonInteractionSystem>().Initialize();
            //ServiceLocator.Current.Get<WorldInputDispatcher>()
                //.Initialize(_session.HUD.actionButton, _session.HUD.attackButton, _session.HUD.targetHPBar);
            
            
            // Spawn managers
            // var spawnManager = FindFirstObjectByType<Systems.Spawn.SpawnManager>();
            // if (spawnManager != null)
            // {
            //     spawnManager.Initialize(context);
            //     context.SpawnManager = spawnManager;
            // }
            //
            // // AI Manager
            // var ai = FindFirstObjectByType<Systems.AI.AIManager>();
            // if (ai != null)
            // {
            //     ai.Initialize(context);
            //     context.AIManager = ai;
            // }
            //
            // // Interaction system
            // var interaction = FindFirstObjectByType<Gameplay.Interaction.InteractionManager>();
            // if (interaction != null)
            // {
            //     interaction.SetPlayer(context.Player);
            //     context.InteractionManager = interaction;
            // }

            Debug.Log("[GameSession] Level systems initialized");
        }
    }
}