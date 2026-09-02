using System.Linq;
using Galactic1.Code.Systems.Economy.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameLoop.States;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core;
using Galactic1.Configs;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using R3;
using UnityEngine;

namespace Galactic1
{
    public class CampEntryPoint : MonoBehaviour
    {
        [SerializeField] private CampRootBinder _worldRootBinder;
        
        

        public Observable<CampExitParams> Run(DIContainer container, CampEnterParams campEnterParams)
        {
            DLog.Alert("Camp scene loaded!", AppConstants.show_log_core);

            var configProvider = container.Resolve<IConfigProvider>();
            var gameContext = container.Resolve<GameSession>().GameLoopContext;
            
            // регистрация способа создания сервисов
            CampRegistrations.Register(container, campEnterParams);
            var campViewModelsContainer = new DIContainer(container);
            CampViewModelsRegistrations.Register(campViewModelsContainer);
            /// после этой строчки можно создавать любой сервис
            ///     >> campViewModelsContainer.Resolve<UICampRootViewModel>();


            
            // === создание портретов юнитов игрока
            ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                configProvider.Get<UnitIdentityPoolConfig>(),
                ServiceLocator.Current.Get<CharacterPreviewService>(),
                gameContext.PlayerUnits.Select(_ => _.ArchetypeId).ToList()
            );
            
            // для таверны
            var tavern = (RecruitmentTavernRuntime)gameContext.GetFacilityByConfigId(GameIdProvider.Tavern);
            ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                ServiceLocator.Current.Get<ConfigProvider>().Get<UnitIdentityPoolConfig>(),
                ServiceLocator.Current.Get<CharacterPreviewService>(),
                tavern.Offers.Select(_ => _.Identity.ArchetypeId).ToList()
            );
            //
            
            
            // === cargo drone, обновляем лимит
            gameContext.Proxy.RemainingDroneCharge.Value = configProvider.Get<EconomyConfig>().CargoDroneMaxCharge;
            
            
            
            // for test
            //InitializeWorld(campViewModelsContainer);
            //InitializeUI(campViewModelsContainer);
            

            var mapEnterParams = new WorldMapEnterParams(0);
            var exitParams = new CampExitParams(mapEnterParams);

            var exitSceneRequest = container.Resolve<Subject<Unit>>(AppConstants.EXIT_SCENE_REQUEST_TAG);
            var exitToMapSceneSignal = exitSceneRequest.Select(_ => exitParams);
            
            return exitToMapSceneSignal;
        }
        
        
        


        void InitializeWorld(DIContainer container)
        {
            _worldRootBinder.Bind(container.Resolve<CampRootViewModel>());
        }

        void InitializeUI(DIContainer container)
        {
            // создали UI для сцены
            var uiRootView = container.Resolve<UIRootView>();
            
            
            // заправшиваем рутовую вью модель и передаем в созданный байндер
            //var uiSceneRootViewModel = viewContainer.Resolve<UICampRootViewModel>();
            //uiSceneRootBinder.Bind(uiSceneRootViewModel);
            
            // can show screens
            //var uiManager = viewContainer.Resolve<CampUIManager>();
            //uiManager.OpenScreenCamp();
        }

        void StateMachine(DIContainer container)
        {
            // создаём состояния
            var states = new IGameLoopState[]
            {
                new CampState(container),
                new CampReportState(container),
                new PreparingSquadState(container),
            };

            var gameSession = container.Resolve<GameSession>();
            var stateMachine = container.Resolve<GameLoopStateMachine>();
            gameSession.GameLoopContext.GameLoopStateMachine = stateMachine;
            stateMachine.Setup(states, gameSession.GameLoopContext);

            // После полной загрузки сцены и инициализации CampEntryPoint:
            // если есть рапорт то запускаем его, тот по окончании перейдет в GameLoopState.Camp
            if (container.Resolve<GameSession>().GameLoopContext.Proxy.HasPendingRaidReport.Value)
                stateMachine.ChangeState(GameLoopState.CampReport);
            else
                stateMachine.ChangeState(GameLoopState.Camp);
        }



        public Coroutine Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            EventBinding<SceneClearEvent> sceneServicesClear = null;
            //GConsole.ClearLog();
            
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
                                //UIScreenId.HUDInput,
                                UIScreenId.HUDCamp,
                                UIScreenId.CampDefenseReport,
                                
                                UIScreenId.BaseConstructionMenu,
                                
                                
                                
                                // === панели для вкладок - ПОРЯДОК НЕ МЕНЯТЬ !!!
                                // (добавлять только в конец)
                                UIScreenId.ManagementScreen, // tab controller
                                // =========================
                                UIScreenId.GameStore,
                                UIScreenId.FacilityList,
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
                    () => container.Resolve<IGameStateProvider>().SaveGameState()
                });
        }
    }
}