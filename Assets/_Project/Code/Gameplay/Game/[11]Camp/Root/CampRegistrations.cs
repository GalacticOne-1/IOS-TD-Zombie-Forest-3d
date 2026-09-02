using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Cameras;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.Gameplay.Construction.States;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.CampDefense.Penalty;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.UI.Buildings;
using Galactic1.Code.UI.Interaction;
using Galactic1.Game.UI.Buildings;
using Galactic1.Items;
using Galactic1.UI.Core;
using Galactic1.UI.Notifications;
using Galactic1.UI.WorldStatus;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1
{
    public static class CampRegistrations
    {
        /// <summary>
        /// В Register для сцены container очищиются перед загрузкой новой сцены,
        /// ничего доп делать не надо!
        /// </summary>
        /// <param name="container"></param>
        /// <param name="campEnterParams"></param>
        /// <exception cref="Exception"></exception>
        public static void Register(DIContainer container, CampEnterParams campEnterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameStateProxy = gameStateProvider.GameStateProxy;
            var configProvider = container.Resolve<IConfigProvider>();
            var gameConfings = configProvider.Get<GameConfig>();
            var gameLoopContext = container.Resolve<GameSession>().GameLoopContext;

            // саб для выхода из сцены
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<Unit>());

            // #1 ОБРАБОТЧК КОММАНД ******************************************************************************
            var cmd = new CommandProcessor(gameStateProvider);
            cmd.RegisterHandle(new CmdCreateWorldsStateHandler(gameStateProxy, gameConfings, configProvider));
            cmd.RegisterHandle(new CmdPlaceEntitiesHandler(gameStateProxy));
            cmd.RegisterHandle(new CmdCratesHandler(gameStateProxy));
            container.RegisterInstance<ICommandProcessor>(cmd);
            
            
            /*      На данный момент мы знаем что пытаемся загрузить карту, но не знаем есть ли ее состояние вообще
             *      Создание карты - это модель, так что с ней нужно работать чрез команды, поэтому нужен обработчик комманд
             *      на случай если состояние карты еще не существует.
             *      Состояние карты должно создаватся до загрузки сцены, что бы не было этих проверок
             */
            var loadingWorldStateId = campEnterParams.WorldStateId;
            var loadingWorldState = gameStateProxy.WorldsState.FirstOrDefault(w => w.Id == loadingWorldStateId);
            if (loadingWorldState == null)
            {
                // Создание состояние через комманду, если его еще нет.
                var command = new CmdCreateWorldState(loadingWorldStateId);

                if (!cmd.Process(command))
                {
                    throw new Exception($"Couldn't create map state with Id {loadingWorldStateId}");
                }

                loadingWorldState = gameStateProxy.WorldsState.First(w => w.Id == loadingWorldStateId);
            }


             
            // #2 СЕРВИСЫ ****************************************************************************************
            
            GameObject.FindObjectOfType<PlayerRootServiceLocatorAssembler>().Initialize(container);
            GameObject.FindObjectOfType<CampServiceLocatorAssembler>().Initialize(container);
            
            ButtonsBinder(container);
            
            
            // === управление режимами в сцене
            var gameModeService = new SceneGameModeService();
            gameModeService.RegisterMode(new NormalGameMode());
            gameModeService.RegisterMode(new ConstructionGameMode());
            gameModeService.SetMode(GameModeType.Normal);
            container.RegisterInstance(gameModeService);
            
            
            // === Interaction Policy
            container.Resolve<InteractionPolicyService>().Reset();
            
            // === UI interaction
            // для каждой сцены устанавливаем базовое состояние
            var uiStateController = container.Resolve<UIStateController>();
            uiStateController.RemoveAll();
            uiStateController.Push(new HUDState());
            
            // === сервис уведомлений
            var notificationService = ServiceLocator.Current.Get<INotificationService>();
            container.RegisterInstance<INotificationService>(notificationService);
            Object.FindAnyObjectByType<UINotificationPresenter>().Initialize(notificationService);
            
            // main camera
            ServiceLocator.Current.Get<CameraController>().Activate();
            
            //ServiceLocator.Current.Get<ViewGameController>().Init();
            //ServiceLocator.Current.Get<StatController>().Init();
            
            
            // ===== ЗАГРУЗКА СОСТОЯНИЙ =====

            
            
            
            
            // === банк-ресурсы для всех источников в лагере
            var inventoryPort = new CampInventoryPort(new List<IInventoryResourcesPort>
            {
                gameLoopContext.CampRuntime.Sources[0] as IInventoryResourcesPort,
                gameLoopContext.PlayerTransport.GetInventory as IInventoryResourcesPort,
            });
            container.RegisterInstance<IInventoryResourcesPort>(inventoryPort);
            
            var constructionRequirementService = new ConstructionRequirementService(inventoryPort);
            container.RegisterFactory(_=> constructionRequirementService).AsSingle();

            
            // состояния для контроллера строительства
            var constructionController = ServiceLocator.Current.Get<ConstructionModeController>();
            container.RegisterInstance(constructionController);

            var repairRequirementService = new RepairRequirementService(
                constructionRequirementService,
                new CeilRepairRoundingStrategy());
            var constructionRepairService = new ConstructionRepairService(
                constructionRequirementService,
                repairRequirementService);
            
            var idle = new IdleState(constructionController, constructionController.Placement);
            var selected = new SelectedObjectState(
                constructionController, 
                constructionController.Placement,
                constructionRepairService);
            var moving = new MovingObjectState(constructionController, constructionController.Placement);
            var placing = new PlacingGhostState(
                constructionController, 
                constructionController.Placement,
                ServiceLocator.Current.Get<CameraController>());

            var stateFactory = new ConstructionStateFactory(
                idle,
                selected,
                moving,
                placing
            );

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
            
            //var blockedMask = GridBlockedMaskGenerator.Build(gridConfig, blockedAreaService);
            //constructionController.GridShaderRenderer.SetBlockedMask(blockedMask);
            constructionController.GridShaderRenderer.InitializeBlockedMask(gridConfig, blockedAreaService);
            
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
            
            constructionController.Initialize(
                stateFactory,
                constructionService,
                container.Resolve<ConstructionRequirementService>(),
                new ConstructionRepairService(
                    constructionRequirementService,
                    repairRequirementService),
                container.Resolve<UIManager>());
            
            //
            
            
            // сделать ссылку на BaseBuildingService ???
            // new BaseFacilityService(
            //     gameLoopContext,
            //     facilityFactory,
            //     GameContent.Facilities.All);
            container.Resolve<FacilitySceneLifecycleSystem>().InitializeScene(facilityFactory, SceneUnitSource.Camp);
            
            container.RegisterFactory(_ => new StructureService(
                loadingWorldState.Entities,
                configProvider.Structures._configs,
                cmd
            )).AsSingle();
            
            
            // === сервисы для зданий
            container.RegisterFactory(_ => new FacilityPresentationAdapter(
                null,
                new FacilityDetailsFactory(
                    container.Resolve<ICampCapacityService>(),
                    container.Resolve<IEconomyService>())
            )).AsSingle();
            
            // === сервис для открытия панелей зданий без самих зданий
            var facilityPanelController = new RuntimeFacilityPanelController(gameLoopContext);
            facilityPanelController.HideTabButton();
            container.RegisterInstance(facilityPanelController);
            
            
            
            
            // === combat servise ability (grande, heal ...)
            var itemUseService = new ItemUseService();
            var coordinator = new AbilityUseCoordinator(
                gameModeService,
                itemUseService);
            
            ServiceLocator.Current.Register(coordinator);
            container.RegisterInstance(itemUseService);
            container.RegisterInstance(coordinator);
            //
            
            
            // *** CLEARING ***
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<AbilityUseCoordinator>();
            }));
        }


        static void ButtonsBinder(DIContainer container)
        {
            // связываем кнопки 
            var buttonsBinder = ServiceLocator.Current.Get<HomeMenuButtonsBinder>();
            
            //buttonsBinder.bGameStore.RegisterButtonClick(container.Resolve<GameStoreService>().ShowWindow);
        }
    }
}