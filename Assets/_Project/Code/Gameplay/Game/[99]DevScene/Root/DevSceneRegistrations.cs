using System;
using System.Linq;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Notification;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.Interaction;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.Notifications;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1
{
    public static class DevSceneRegistrations
    {
        public static void Register(DIContainer container, DevSceneEnterParams devSceneEnterParams)
        {
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameStateProxy = gameStateProvider.GameStateProxy;
            var configProvider = container.Resolve<IConfigProvider>();
            var gameConfings = configProvider.Get<GameConfig>();

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
            var loadingWorldStateId = devSceneEnterParams.WorldStateId;
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
            GameObject.FindObjectOfType<DevSceneServiceLocatorAssembler>().Initialize(container);
            
            var gameLoopContext = container.Resolve<GameSession>().GameLoopContext;

            var combatTargetingService = GameObject.FindAnyObjectByType<CombatTargetingService>();
            
            // === управление режимами в сцене
            var gameModeService = new SceneGameModeService();
            gameModeService.RegisterMode(new NormalGameMode());
            gameModeService.RegisterMode(new RaidGameMode());
            gameModeService.RegisterMode(new AbilityTargetingGameMode(new CombatTargetingAdapter(combatTargetingService)));
            gameModeService.SetMode(GameModeType.Raid);
            container.RegisterInstance(gameModeService);
            
            // === Interaction Policy
            container.Resolve<InteractionPolicyService>().Reset();

            // === обработчик ввода для абилок в бою
            var inputPipeline = new TargetingInputPipeline(ServiceLocator.Current.Get<WorldInputDispatcher>());
            combatTargetingService.Initialize(inputPipeline);
            
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
            
            var inventoryPort = new LocationInventoryPort(gameLoopContext.PlayerTransport.GetInventory as IInventoryResourcesPort);
            container.RegisterInstance<IInventoryResourcesPort>(inventoryPort);
            
            // === RAID
            var squadScene = new SquadSceneRuntime();
            container.RegisterInstance(squadScene);
            ServiceLocator.Current.Get<SquadController>().Initialize(
                squadScene,
                ServiceLocator.Current.Get<WorldInputDispatcher>());
            
            
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
    }
}