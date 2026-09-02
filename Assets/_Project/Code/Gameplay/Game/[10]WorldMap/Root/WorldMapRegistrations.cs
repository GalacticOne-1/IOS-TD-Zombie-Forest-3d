using System;
using System.Linq;
using Galactic1.Code.Cameras;
using Galactic1.Code.Game.Rewards;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Notification;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.UI.Buildings;
using Galactic1.Code.UI.Interaction;
using Galactic1.Code.UI.RaidReport;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.UI.Buildings;
using Galactic1.UI.Notifications;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1
{
    public static class WorldMapRegistrations
    {
        public static void Register(DIContainer container, WorldMapEnterParams enterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameStateProxy = gameStateProvider.GameStateProxy;
            var configProvider = container.Resolve<IConfigProvider>();
            var gameConfings = configProvider.Get<GameConfig>();
            var gameContext = container.Resolve<GameSession>().GameLoopContext;

            // саб для выхода из сцены
            container.RegisterInstance(AppConstants.EXIT_SCENE_REQUEST_TAG, new Subject<Unit>());

            // #1 ОБРАБОТЧК КОММАНД ******************************************************************************
            var cmd = new CommandProcessor(gameStateProvider);
            cmd.RegisterHandle(new CmdCreateWorldsStateHandler(gameStateProxy, gameConfings, configProvider));
            container.RegisterInstance<ICommandProcessor>(cmd);
            
            
            /*      На данный момент мы знаем что пытаемся загрузить карту, но не знаем есть ли ее состояние вообще
             *      Создание карты - это модель, так что с ней нужно работать чрез команды, поэтому нужен обработчик комманд
             *      на случай если состояние карты еще не существует.
             *      Состояние карты должно создаватся до загрузки сцены, что бы не было этих проверок
             */
            var loadingWorldStateId = enterParams.WorldStateId;
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
            
            GameObject.FindObjectOfType<WorldMapServiceLocatorAssembler>().Initialize(container);
            
            var gameLoopContext = container.Resolve<GameSession>().GameLoopContext;
            
            // === управление режимами в сцене
            var gameModeService = new SceneGameModeService();
            gameModeService.RegisterMode(new NormalGameMode());
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
            ServiceLocator.Current.Get<IMainCamera>().Activate();

            
            var inventoryPort = new LocationInventoryPort(gameLoopContext.PlayerTransport.GetInventory as IInventoryResourcesPort);
            container.RegisterInstance<IInventoryResourcesPort>(inventoryPort);

            // === raid report
            ServiceLocator.Current.Get<RaidReportFlowController>().Initialize(
                gameContext,
                gameContext.PlayerTransport.GetInventory,
                ServiceLocator.Current.Get<IAdRewardProvider>(),
                configProvider);
            
            
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
        }
    }
}