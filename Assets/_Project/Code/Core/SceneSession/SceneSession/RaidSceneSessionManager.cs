  
using System.Collections.Generic;
using Galactic1;
using Galactic1.Code.Cameras;
using Galactic1.Code.Cameras.Configs;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Squad;
using Galactic1.Configs;
using Galactic1.Core.UI;
using Galactic1.Core.UI.HUD;
using Galactic1.Gameplay.Death;
using Galactic1.Gameplay.Locations;
using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.Gameplay.Locations.Navigation;
using Galactic1.UI.Core;
using Galactic1.UI.WorldStatus;
using UnityEngine;


namespace Galactic1.Core.Systems.GameSession
{
    public class RaidSceneSessionManager : SceneSessionManager
    {
        
        
        protected override void Initialize(DIContainer container)
        {
            // #1 
            var configProvider = _container.Resolve<IConfigProvider>();
            var gameLoopContext = gameSession.GameLoopContext;
            var options = gameLoopContext.CurrentRaid.Scenario.Options;
            
            // #2 Загружаем локацию
            var locationLoader = FindAnyObjectByType<LocationLoader>();
            if (locationLoader != null)
                locationLoader.Load(out _session, _container);
            
            // #3 устанавливаем размер сетки
            var geometry = FindAnyObjectByType<LocationGeometryDefinition>();
            var configurationDto = geometry.GetDto();

            
            // === для режима орды спавним объекты лагеря
            if (options.UseDefenseFacilities)
            {
                var gridConfig = configProvider.Get<GridSettingsConfig>();
                var coordinateService = new GridCoordinateService(
                    gridConfig.CellSize,
                    gridConfig.GridOffset,
                    gridConfig.GridSize
                );
                var blockedAreaService = new GridBlockedAreaService(configProvider.Get<GridBlockedAreasConfig>());
                var constructionService = new ConstructionService(
                    new GridService(),
                    coordinateService,
                    blockedAreaService);

                // factory
                var worldStatusFactory = new WorldStatusFactory(
                    ServiceLocator.Current.Get<CameraController>().Camera,
                    container.Resolve<UIManager>().TransformRoot.hudWorldRoot.CMP_RectTr()
                );
                var facilityFactory = new FacilityFactory(
                    constructionService,
                    coordinateService,
                    worldStatusFactory);
                container.RegisterFactory(_ => facilityFactory).AsSingle();

                // new BaseFacilityService(
                //     gameLoopContext,
                //     facilityFactory,
                //     GameContent.Facilities.All);
                container.Resolve<FacilitySceneLifecycleSystem>()
                    .InitializeScene(facilityFactory, SceneUnitSource.CampDefense);
                
                
                // *** места спавна для защитников лагеря
                var hqInstance = FindAnyObjectByType<CampHQInstance>();
                Vector3[] campDefspawn = new Vector3[0];
                if(hqInstance)
                {
                    var spawnPointRoot = hqInstance.UnitSpawnPointRoot;
                    var l = spawnPointRoot.childCount;
                    campDefspawn = new Vector3[l];
                    for (int i = 0; i < l; i++)
                        campDefspawn[i] = spawnPointRoot.GetChild(i).position;
                }
                _session.LocationContext.CampUnitSpawnPosition = campDefspawn;
            }

            new LocationNavigationSystem().Configure(configurationDto);
            
            // * передаем временный инвентарь транспорта
            // (оружие при перезарядке берет патроны здесь)
            // если игрок проходит локацию то этот инвентарь перезаписывает рабочий

            // #1 для защиты лагеря используем оба инвентаря
            if (options.UseDefenseFacilities)
            {
                _session.InventoryPort = new CampInventoryPort(new List<IInventoryResourcesPort>
                {
                    // * расход патронов в первую очередь будет из транспорта
                    gameLoopContext.CurrentRaid.PlayerTransport.Sources.Cargo as IInventoryResourcesPort,
                    gameLoopContext.CurrentRaid.CampRuntime.Sources.Cargo as IInventoryResourcesPort,
                });
            }
            // #2 в обычном рейде только инвентарь транспорта
            else
            {
                _session.InventoryPort = 
                    gameLoopContext.CurrentRaid.PlayerTransport.Sources.Cargo as IInventoryResourcesPort;
            }
            
            
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
            var options = gameLoopContext.CurrentRaid.Scenario.Options;

            // spawn survivors
            // var cameraTargetGroup = ServiceLocator.Current.Get<CameraTargetGroup>();
            // cameraTargetGroup.Clear();
            //
            // var playerFactory = ServiceLocator.Current.Get<PlayerFactory>();
            //
            // // === спавним всех юнитов игрока
            // var raidUnitRuntime = gameLoopContext.CurrentRaid.Squad.Units;
            // var l = raidUnitRuntime.Count;
            // _session.Survivors = new(l);
            // for (int i = 0; i < l; i++)
            // {
                // var survivor = playerFactory.Create(i, _session, null, raidUnitRuntime[i]);
                // DLog.Alert($"Spawning survivor => {raidUnitRuntime[i].MetaUnit.Proxy.Id}", EDlogColor.YELLOW);
                //
                // // сервис фокуса камеры на отряде
                // cameraTargetGroup.Add(survivor.tr);
                // survivor.OnDeath += () => cameraTargetGroup.Remove(survivor.tr);
                // survivor.OnDestory += () => cameraTargetGroup.Remove(survivor.tr);
            //}
            
            
            
            // ===================================================================================================
            // === создание представления в сцене
            
            
            if (options.UseTransport)
                _container.Resolve<TransportSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Raid);
            
            // === player squad
            _container.Resolve<UnitSceneLifecycleSystem>().InitializeScene(_session, SceneUnitSource.Raid);
            _container.Resolve<UnitSceneLifecycleSystem>().ActivateScene();
            
            // === scene runtime
            var squadScene = _container.Resolve<SquadSceneRuntime>();
    
            foreach (var survivor in _session.Survivors)
            {
                // подключение юнитов под управление игрока, только для отряда
                if (!survivor.UnitAdapter.Runtime.IsCampDefender) 
                    squadScene.AddAgent(survivor);
            }
            //
            
            
            
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