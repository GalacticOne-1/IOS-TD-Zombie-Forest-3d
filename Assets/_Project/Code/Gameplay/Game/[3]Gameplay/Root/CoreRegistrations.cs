
using Galactic1.Code.Cameras;
using Galactic1.Code.Game.Rewards;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Enemies.Factories;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.CampDefense.Penalty;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.Daily;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Economy.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.ProductionPipeline;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.Survival;
using Galactic1.Code.Systems.Tutorial.Analytics;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Objectives;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Code.UI.Interaction;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.Gameplay;
using Galactic1.Core.Systems;
using Galactic1.Core.Systems.Factories;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.UI.Stats;
using Galactic1.Items;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.PoolObject;
using Galactic1.UI.Shop;
using Galactic1.Systems;
using Galactic1.UI.Core;
using UnityEngine.EventSystems;

namespace Galactic1
{
    public static class CoreRegistrations
    {
        /// <summary>
        /// Вызов в один раз в начале
        /// Порядоk вызовов в GameEntryPoint
        /// <br/>1. Servicelocator (just link)
        /// <br/>2. GameStateProvider (Loading data)
        /// <br/>3. CoreRegistrations
        /// </summary>
        /// <param name="rootContainer"></param>
        public static void Register(DIContainer rootContainer, Coroutines coroutines)
        {
            var gameStateProvider = rootContainer.Resolve<IGameStateProvider>();
            var gameStateProxy = gameStateProvider.GameStateProxy;
            var configProvider = rootContainer.Resolve<IConfigProvider>();
            var gameConfings = configProvider.Get<GameConfig>();

            
            // #1 ОБРАБОТЧК КОММАНД ******************************************************************************
            var cmd = new CommandProcessor(gameStateProvider);
            cmd.RegisterHandle(new CmdBankResourcesAddHandler(gameStateProxy));
            cmd.RegisterHandle(new CmdBankResourcesSpendHandler(gameStateProxy));
            rootContainer.RegisterInstance<ICommandProcessor>(cmd);
            
            // ***************************************************************************************************
            
            // === доступ к сохранению
            ServiceLocator.Current.Register(gameStateProvider);
            
            
            // === layer service
            var layerConfig = configProvider.Get<LayerConfig>();
            var layerService = new LayerService(layerConfig);
            rootContainer.RegisterInstance(layerService);
            ServiceLocator.Current.Register(layerService);
            
            
            DeveloperConsole.I.Activator();
            var mbm = ServiceLocator.Current.Get<MonoBehaviourMaster>();
            mbm.Activate();
            
            // === обработка потока ввода
            var inputRouter = new UIInputRouter(EventSystem.current);
            rootContainer.RegisterInstance(inputRouter);
            ServiceLocator.Current.Get<UIInputRouterBehaviour>().Initialize(inputRouter, mbm);
            
            // === для управления кнопками
            ServiceLocator.Current.Register(new UIBlockService());
            rootContainer.RegisterInstance(new UIStateController(
                new UILayerController(),
                new UIInteractionLockService()
            ));

            
            
            // === окно с настройками и сам контроллер
            ServiceLocator.Current.Get<SettingsManager>().Activate();
            rootContainer.RegisterInstance(ServiceLocator.Current.Get<SettingsManager>());
            rootContainer.RegisterInstance(new GameSettingsSystem(rootContainer));   // нужен для создания окна
            ServiceLocator.Current.Get<AudioManager>().Activate();
            
            
            // ****************************************************************************************************
            // ****************************************************************************************************
            // GAMEPLAY
            
            
            
            // !!! ВАЖНО. НИЧЕГО НЕ УДАЛЯТЬ !!!
            // === GAME LOOP ===========================================================================================
            
            /*
             *  регистрируем машину состояний для core loop
             *  состояние устанавливается в ...EntryPoint загруженной сцены
             */
            rootContainer.RegisterInstance(new GameLoopStateMachine());
            
            // == game session (GameLoopContext)
            var gameSession = ServiceLocator.Current.Get<GameSession>();
            rootContainer.RegisterInstance(gameSession);
            gameSession.Initialize(rootContainer);
            
            /* регистрируем события для смены сцен */
            EventBus<WorldMapSceneRequestEvent>.Register(new EventBinding<WorldMapSceneRequestEvent>(_ =>
                ServiceLocator.Current.Get<LocationTransitionService>()
                    .GoToLocation(-1, new ())));
            
            EventBus<LocationSceneRequestEvent>.Register(new EventBinding<LocationSceneRequestEvent>(_ =>
                ServiceLocator.Current.Get<LocationTransitionService>()
                    .GoToLocation(_.LocationId, new ())));

            
            EventBus<HomeSceneRequestEvent>.Register(new EventBinding<HomeSceneRequestEvent>(_ =>
                ServiceLocator.Current.Get<LocationTransitionService>()
                    .GoToLocation(0, new()
                    {
                        ResetRootPlayerScene = _.ResetRootPlayerScene
                    })));

            // для входа в лагерь в режиме орды
            EventBus<CampDefenseRequestEvent>.Register(new EventBinding<CampDefenseRequestEvent>(_ =>
                ServiceLocator.Current.Get<LocationTransitionService>()
                    .GoToLocation(0, new()
                    {
                        CampDefense = true,
                    })));
            
            // === meta progress
            // ...
            
            
            // =========================================================================================================
            // =========================================================================================================


            // === audio for UI all scenes
            rootContainer.RegisterInstance(new UIAudioSystem());
            
            
            // !!! ПОРЯДОК НЕ МЕНЯТЬ !!!
            
            // === #1 сервис времени
            var interactionBlocker = new SceneInteractionBlocker();
            ServiceLocator.Current.Register(interactionBlocker);
            
            var timeScaleService = new GameTimeScaleService();
            rootContainer.RegisterInstance(timeScaleService);
            rootContainer.RegisterInstance(new GamePauseService(timeScaleService, interactionBlocker));
            rootContainer.RegisterInstance(ServiceLocator.Current.Get<TimeBoundaryService>());
            
            ServiceLocator.Current.Register(rootContainer.Resolve<GameTimeScaleService>());
            ServiceLocator.Current.Register(rootContainer.Resolve<GamePauseService>());
            
            // === #2 Bank 
            //rootContainer.RegisterFactory(_ => new BankResourceService(gameStateProxy.BankResources, cmd)).AsSingle();
            var economyConfig = configProvider.Get<EconomyConfig>();
            rootContainer.RegisterInstance<IEconomyService>(new EconomyService(
                new CurrencyRuntime(gameStateProxy.BankResources),
                new ProductionSkipCostService(economyConfig),
                new DroneCostService(economyConfig)));
            ServiceLocator.Current.Register(rootContainer.Resolve<IEconomyService>());
            
            // === #3 IAP service
            rootContainer.RegisterFactory(_ => new GameStoreService(
                rootContainer,
                gameStateProxy.IAPCardsProxy,
                configProvider.IAP._configs)
            ).AsSingle();

            // === #4 reward service
            RewardService(rootContainer);
            
            
            // === #5 stat style
            rootContainer.RegisterInstance(new StatStyleResolver(configProvider.Get<StatStyleConfig>()));
            
            // === #6 widget queue
            rootContainer.RegisterInstance(new WidgetQueueService(coroutines));
            
            // === #7 сервис определяющий правила для взаимодействия с объектами в локации
            var interactionPolicy = new InteractionPolicyService();
            rootContainer.RegisterInstance(interactionPolicy);
            ServiceLocator.Current.Register(interactionPolicy);
            
            
            // === Tutorial =============================================================
            var tutorialGameStateQuery = new TutorialGameStateQuery(
                gameSession.GameLoopContext,
                rootContainer.Resolve<GameLoopStateMachine>());

            var tutorialObjectiveFactory = new TutorialObjectiveFactory(
                tutorialGameStateQuery,  // ITutorialInventoryQuery
                tutorialGameStateQuery,  // ITutorialSquadQuery
                tutorialGameStateQuery); // IGameLoopStateQuery

            var tutorialCheckpointService = new TutorialCheckpointService();
            var tutorialInputPolicyService = new TutorialInputPolicyService(interactionPolicy);

            var tutorialTargetRegistry = new TutorialTargetRegistry();
            rootContainer.RegisterInstance(tutorialTargetRegistry);
            ServiceLocator.Current.Register(tutorialTargetRegistry);

            var tutorialPresentationService = new TutorialPresentationService(tutorialTargetRegistry);
            rootContainer.RegisterInstance(tutorialPresentationService);
            ServiceLocator.Current.Register(tutorialPresentationService);

            var tutorialService = new TutorialService(
                configProvider.Get<TutorialCampaignRegistry>(),
                tutorialObjectiveFactory,
                tutorialCheckpointService,
                tutorialGameStateQuery,
                tutorialInputPolicyService,
                tutorialPresentationService,
                new NullTutorialAnalytics(),
                gameStateProvider,
                gameStateProvider.GameStateProxy.Tutorial);

            rootContainer.RegisterInstance<ITutorialService>(tutorialService);
            rootContainer.RegisterInstance<ITutorialDebugService>(tutorialService);
            ServiceLocator.Current.Register<ITutorialService>(tutorialService);
#if UNITY_EDITOR
            ServiceLocator.Current.Register<ITutorialDebugService>(tutorialService);
#endif
            
            // ===========================================================================
            
            
            
            // =========================================================================================================
            // =========================================================================================================
            
            // === AoE
            ServiceLocator.Current.Register(new AoEService(layerService));
            ServiceLocator.Current.Register(new TemporalAoEService());

            // глобальный пул
            new GlobalPoolRegistry(
                ServiceLocator.Current.Get<PoolManager>(),
                configProvider.Get<ObjectPoolConfigs>());
            
            
            // ==== время игрового мира и угроза, глобальны на весь проект
            var gameTime = ServiceLocator.Current.Get<GameTimeService>();
            gameTime.Activate(rootContainer);
            rootContainer.RegisterInstance(gameTime);
            
            var worldThreatService = ServiceLocator.Current.Get<WorldThreatService>();
            worldThreatService.Activate(rootContainer);
            rootContainer.RegisterInstance(worldThreatService);
            
            // === Camp Defense — координатор готовности защиты лагеря.
            // Активируется ПОСЛЕ WorldThreatService: подписывается на его события
            // и сразу вычисляет состояние по уже существующей/загруженной угрозе.
            var campDefensePreparation = new CampDefensePreparationService();
            rootContainer.RegisterInstance(campDefensePreparation);
            rootContainer.RegisterInstance(new CampDefenseImmediateDefeatService(rootContainer, gameSession.GameLoopContext));
            campDefensePreparation.Activate(rootContainer);

            // === сервис расчета потерь за провал защиты лагеря
            var campDefenseFailureService = new CampDefenseFailureService(
                new CampDefensePenaltyCalculator(configProvider.Get<CampDefensePenaltyConfig>()),
                new CampDefensePenaltyApplier());
            rootContainer.RegisterInstance(campDefenseFailureService);
            //
            
            
            // === Camp services
            var inboxService = new InboxService(
                gameSession.GameLoopContext.InboxRuntime,
                gameTime);
            rootContainer.RegisterInstance(inboxService);
            ServiceLocator.Current.Register(inboxService);
            
            var storageRegistry = new StorageRegistry();
            rootContainer.RegisterInstance(storageRegistry);
            
            var facilityService = new FacilityRuntimeService(
                gameSession,
                gameTime,
                rootContainer.Resolve<IEconomyService>(),
                configProvider,
                storageRegistry
            );
            var campCapacity = new CampCapacityService(gameSession.GameLoopContext, facilityService);
            
            rootContainer.RegisterInstance<IFacilityRuntimeService>(facilityService);
            ServiceLocator.Current.Register<IFacilityRuntimeService>(facilityService);
            
            rootContainer.RegisterInstance<ICampCapacityService>(campCapacity);
            ServiceLocator.Current.Register<ICampCapacityService>(campCapacity);

            ServiceLocator.Current.Register<IIdentityGenerator>(
                new DefaultIdentityGenerator(configProvider.Get<UnitIdentityPoolConfig>()));
            ServiceLocator.Current.Register<IWeightedRandomService>(new DefaultWeightedRandomService());
            
            ServiceLocator.Current.Register<IRecruitEquipmentGenerator>(new RecruitEquipmentGenerator(
                configProvider.Get<ItemDatabase>(),
                configProvider.Get<RecruitmentDatabase>(),
                configProvider.Get<RecruitEquipmentAccessConfig>(),
                ServiceLocator.Current.Get<IWeightedRandomService>()
            ));
            
            
            // ************
            facilityService.Initialize();
            rootContainer.RegisterInstance(new AutoCollectPipeline(
                storageRegistry,
                (IInventoryResourcesPort)gameSession.GameLoopContext.CampRuntime.Sources[0],
                configProvider.Get<ItemDatabase>()
            ));

            rootContainer.RegisterFactory<ISceneAdapterFactory>(_ =>
                new SceneAdapterFactory(
                    _.Resolve<IEconomyService>(),
                    configProvider.Get<ItemDatabase>(),
                    gameSession.GameLoopContext,
                    storageRegistry)
            ).AsSingle();
            // 
            
            
            // === Transport Lifecycle System
            var transportFactory = new TransportFactory();
            ServiceLocator.Current.Register(transportFactory);

            rootContainer.RegisterInstance(new TransportSceneLifecycleSystem(
                gameSession.GameLoopContext,
                transportFactory
            ));
            //
            
            // === Сервис расхода провизии юнитами игрока
            rootContainer.RegisterInstance(new SurvivorDailyConsumptionService(
                gameSession.GameLoopContext,
                gameTime,
                configProvider.Get<SurvivorConsumptionConfig>()));
            
            // === unit lifecycle system
            var playerFactory = new PlayerFactory();
            ServiceLocator.Current.Register(playerFactory);

            rootContainer.RegisterInstance(new UnitSceneLifecycleSystem(
                gameSession.GameLoopContext,
                playerFactory,
                configProvider.Get<UnitIdentityPoolConfig>(),
                configProvider.Get<WeaponAnimLibrary>(),
                ServiceLocator.Current.Get<SurvivorRepository>(),
                ServiceLocator.Current.Get<CameraTargetGroup>()
            ));
            //

            // === facility lifecycle system
            rootContainer.RegisterInstance(new FacilitySceneLifecycleSystem(
                gameSession.GameLoopContext,
                GameContent.Facilities.All
            ));
            //
            
            // === enemy 
            rootContainer.RegisterInstance(new EnemyRuntimeFactory());
            rootContainer.RegisterInstance(new ZombieFactory());
            //

            
            
            // ************************************************************************************************
            // ************************************************************************************************
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(() =>
            {
                // очищаем рейдовый пул
                ServiceLocator.Current.Get<PoolManager>().ClearScene();
                
            }));
        }


        static void RewardService(DIContainer rootContainer)
        {
            var configProvider = rootContainer.Resolve<IConfigProvider>();
            
            var rewardService = new RewardService(rootContainer);
            var adRewardConfigProvider = new AdRewardConfigProvider(configProvider.Get<AdRewardsConfig>());
            
            rootContainer.RegisterInstance(rewardService);
            ServiceLocator.Current.Register(rewardService);
            
            rootContainer.RegisterInstance<IAdRewardProvider>(adRewardConfigProvider);
            ServiceLocator.Current.Register<IAdRewardProvider>(adRewardConfigProvider);
        }
    }
}