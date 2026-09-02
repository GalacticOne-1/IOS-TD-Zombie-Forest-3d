using System.Linq;
using Galactic1.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameLoop.States;
using Galactic1.Code.WorldMap;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using R3;
using UnityEngine;

namespace Galactic1
{
    public class WorldMapEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIWorldMapRootBinder sceneUIWorldRootPrefab;
        

        public Observable<WorldMapExitParams> Run(DIContainer container, WorldMapEnterParams worldMapEnterParams)
        {
            DLog.Alert("Map scene loaded!", AppConstants.show_log_core);
            
            var configProvider = container.Resolve<IConfigProvider>();
            var gameContext = container.Resolve<GameSession>().GameLoopContext;
            
            // регистрация способа создания сервисов
            WorldMapRegistrations.Register(container, worldMapEnterParams);
            var mapViewModelsContainer = new DIContainer(container);
            WorldMapViewModelsRegistrations.Register(mapViewModelsContainer);
            /// после этой строчки можно создавать любой сервис
            ///     >> mapViewModelsContainer.Resolve<UICampRootViewModel>();

            //var uiRootView = mapContainer.Resolve<UIRootView>();
            //var uiScene = Instantiate(sceneUIWorldRootPrefab);
            //uiRootView.AttachSceneUI(uiScene.gameObject);
            
            
            // === создание портретов юнитов игрока
            ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                configProvider.Get<UnitIdentityPoolConfig>(),
                ServiceLocator.Current.Get<CharacterPreviewService>(),
                gameContext.PlayerUnits.Select(_ => _.ArchetypeId).ToList(),
                () => { Debug.Log("[Preview] All portraits ready"); }
            );

            
            var exitSignalSubj = new Subject<Unit>();
            //uiScene.Bind(exitSignalSubj);

            
            var campEnterParams = new CampEnterParams(0);
            var mapExitParams = new WorldMapExitParams(campEnterParams);
            var exitToCampSceneSignal = exitSignalSubj.Select(_ => mapExitParams);

            return exitToCampSceneSignal;
        }
        
        void StateMachine(DIContainer container)
        {
            // создаём состояния
            var states = new IGameLoopState[]
            {
                new WorldMapState(container, ServiceLocator.Current.Get<WorldMapController>()),
                new RaidLaunchingState(container)
            };

            var gameSession = container.Resolve<GameSession>();
            var stateMachine = container.Resolve<GameLoopStateMachine>();
            gameSession.GameLoopContext.GameLoopStateMachine = stateMachine;
            stateMachine.Setup(states, gameSession.GameLoopContext);

            // После полной загрузки сцены и инициализации WorldMapEntryPoint:
            stateMachine.ChangeState(GameLoopState.WorldMap);
        }
        
        
        public Coroutine Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            EventBinding<SceneClearEvent> sceneServicesClear = null;
            GConsole.ClearLog();
            
            return ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.1f,
                new DFunc[]
                {
                    /*
                     *  После загрузки сцены загружаем нужные окна и пр
                     *  >> campContainer.Resolve<UIManager>().ScreenManager.PreloadScreens
                     *  (регистрация в ServiceLocator происходит в самом окне после создания)
                     *
                     *  Сразу регистрируем очистку всего загруженного
                     */
                    // #1 UI
                    () =>
                    {
                        // === грузим окна для сцены
                        container.Resolve<UIManager>().ScreenManager.PreloadScreens(
                            container,
                            new[]
                            {
                                UIScreenId.Settings,
                                UIScreenId.PurchaseRewardScreen,
                                UIScreenId.Review,
                                UIScreenId.HUDMap,
                                UIScreenId.LocationOverview,
                                UIScreenId.RaidReport,
                                UIScreenId.CampDefenseMapReport,

                                
                                // === панели для вкладок - ПОРЯДОК НЕ МЕНЯТЬ !!!
                                // (добавлять только в конец)
                                UIScreenId.ManagementScreen, // tab controller
                                // =========================
                                UIScreenId.GameStore,
                                UIScreenId.FacilityPanel,
                                UIScreenId.Inventory,
                                //
                            });
                        
                        // === грузим попапы
                        container.Resolve<UIManager>().PopupManager.Preload(new []
                        {
                            UIScreenId.AdAlertToast,
                            UIScreenId.ConfirmPopup
                        });
                        
                        // *** очистка созданных окон и их зависимостей
                        EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(_ =>
                        {
                            
                            // -- ! в конце после отписки от ServiceLocator ! --
                            container.Resolve<UIManager>().ScreenManager.RemoveScreens();
                            container.Resolve<UIManager>().PopupManager.RemovePopups();
                        }));
                    },
                    
                    
                    /*
                     *
                     */
                    // #2 WORLD
                    () =>
                    {
                        /* =================================================================
                         * -> StateMachine управляет логикой.
                         * -> SceneSessionManager управляет объектами Unity.
                         */
                        ServiceLocator.Current.Get<SceneSessionManager>().START(container);
                        StateMachine(container);
                        // =================================================================
                    },
                    () =>
                    {
                        
                    },

                    // -- ! в конце ! --
                    () => container.Resolve<IGameStateProvider>().SaveGameState()
                });
        }
    }
}