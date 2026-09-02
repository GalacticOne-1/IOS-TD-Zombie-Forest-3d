using System.Linq;
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameLoop.States;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.PoolObject;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using R3;
using UnityEngine;

namespace Galactic1
{
    public class LocationEntryPoint : MonoBehaviour
    {
        [SerializeField] private UILocationRootBinder _sceneUIRootPrefab;
        
        
        public Observable<LocationExitParams> Run(DIContainer container, LocationEnterParams locationEnterParams)
        {
            DLog.Alert("Location scene loaded!", AppConstants.show_log_core);
            
            var configProvider = container.Resolve<IConfigProvider>();
            var gameContext = container.Resolve<GameSession>().GameLoopContext;

            // регистрация способа создания сервисов
            LocationRegistrations.Register(container, locationEnterParams);
            var locationViewModelsContainer = new DIContainer(container);
            //CampViewModelsRegistrations.Register(locationViewModelsContainer);
            /// после этой строчки можно создавать любой сервис
            ///     >> campViewModelsContainer.Resolve<UICampRootViewModel>();
            
            
            
            // === создание портретов юнитов игрока
            ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                configProvider.Get<UnitIdentityPoolConfig>(),
                ServiceLocator.Current.Get<CharacterPreviewService>(),
                gameContext.PlayerUnits.Select(_ => _.ArchetypeId).ToList(),
                () => { Debug.Log("[Preview] All portraits ready"); }
            );
            
            
            // === raid pool
            new RaidPoolRegistry(
                ServiceLocator.Current.Get<PoolManager>(),
                configProvider.Get<ObjectPoolConfigs>());
            
            // === AoE zone service
            ServiceLocator.Current.Get<TemporalAoEService>().Initialize();
            
            // === очищаем список под новый рейд
            TargetInfoRegistry.Clear();
            HitboxRegistry.Clear();
            
            

            var mapEnterParams = new WorldMapEnterParams(0);
            var exitParams = new LocationExitParams(mapEnterParams);
            
            var exitSceneRequest = container.Resolve<Subject<Unit>>(AppConstants.EXIT_SCENE_REQUEST_TAG);
            var exitToMapSceneSignal = exitSceneRequest.Select(_ => exitParams);
            
            return exitToMapSceneSignal;
        }
        
        
        
        void StateMachine(DIContainer container)
        {
            // создаём состояния
            var states = new IGameLoopState[]
            {
                new RaidInProgressState(container),
                new RaidResolvingState(container),
                new PostRaidReportState(container)
            };

            var gameSession = container.Resolve<GameSession>();
            var stateMachine = container.Resolve<GameLoopStateMachine>();
            gameSession.GameLoopContext.GameLoopStateMachine = stateMachine;
            stateMachine.Setup(states, gameSession.GameLoopContext);

            // После полной загрузки сцены и инициализации LocationEntryPoint:
            stateMachine.ChangeState(GameLoopState.RaidInProgress);
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
                                UIScreenId.DeathScreen,
                                UIScreenId.HUDInput,
                                UIScreenId.HUDLocation,
                                
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
                        StateMachine(container);
                        ServiceLocator.Current.Get<SceneSessionManager>().START(container);
                        // =================================================================
                    },
                    () =>
                    {
                        
                    },

                    // -- ! в конце ! --
                    () => _GameState.Save()
                });
        }
    }
}