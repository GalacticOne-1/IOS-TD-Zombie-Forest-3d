using System.Collections;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.Ads;
using Galactic1.Code.Systems.Ads.AdMob;
using Galactic1.Code.WorldMap;
using Galactic1.Configs;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Core;
using Galactic1.Core.Systems;
using Galactic1.EntryPoint;
using Galactic1.Gameplay.Death;
using Galactic1.Localisation;
using Galactic1.Mobile;
using Galactic1.Systems;
using Galactic1.Systems.Privacy;
using Galactic1.UI.Shop;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Galactic1
{
    public class GameEntryPoint
    {
        private static GameEntryPoint _instance;
        private Coroutines _coroutines;
        private UIRootView _uiRoot;
        private readonly DIContainer _rootContainer = new();
        private DIContainer _cashedSceneContainer;


        private GameObject _core;
        private GameObject _systems;
        
        /*
         *      Этот метод всегда запускается первым, независимо от сцены
         *      здесь стартует приложение
         */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoStartGame()
        {
            // *** для установки системных значений
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;          // << ???
            //ScreenSizeController.SetScreenSize();
            
            
            // ****************************************************************************************************
            // ****************************************************************************************************
            // ****************************************************************************************************
            
            
            // >>> LAUNCH <<<
            _instance = new GameEntryPoint();
            //_instance.StartGame();
        }

        
        /*
         *      Здесь загружаем все глобальные зависимости которые не удаляются (former scene CORE)
         */
        private GameEntryPoint()
        {
            // #1
            _coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
            Object.DontDestroyOnLoad(_coroutines.gameObject);
            
            // #2
            _core = new GameObject("[CORE]");
            Object.DontDestroyOnLoad(_core.gameObject);
            
            // #3
            _systems = new GameObject("[SYSTEMS]");
            Object.DontDestroyOnLoad(_systems.gameObject);
            _systems.AddComponent<LoadingManager>();

            // #4
            _uiRoot = "Prefabs/UI/Root/UIRoot".CreateGO(null).GetComponent<UIRootView>();
            Object.DontDestroyOnLoad(_uiRoot.gameObject);
            _rootContainer.RegisterInstance(_uiRoot);
            
            
            
            
            // *** build version
            ResourceRequest request = Resources.LoadAsync("Build", typeof(BuildScriptableObject));
            request.completed += operation =>
            {
                BuildScriptableObject buildScriptableObject = ((ResourceRequest)operation).asset as BuildScriptableObject;

                if (buildScriptableObject == null)
                {
                    Debug.LogError("Build scriptable object not found in resources directory! Check build log for errors!");
                }
                else
                {
                    SystemRepository.AppVersion = $"{Application.version}.{buildScriptableObject.BuildNumber}";
                    _uiRoot.SetVersion(SystemRepository.AppVersion);
                }
            };

            
            // ****************************************************************************************************
            // ****************************************************************************************************
            // ****************************************************************************************************
            

            _coroutines.StartCoroutine(Bootstrap());
        }

        IEnumerator Bootstrap()
        {
            _coroutines.StartCoroutine(LoadingManager.I.FakeProgressToFive());
            
            // if (AppConstants.SERVER_ON)
            // {
            //     // 1️⃣ Проверяем соединение с сервером
            //     bool connected = false;
            //     yield return _coroutines.StartCoroutine(ServerAPI.I.PingServer((ok) => connected = ok));
            //
            //     if (!connected)
            //     {
            //         DLog.Alert("Нет соединения с сервером!", EDlogColor.RED);
            //         yield break;
            //     }
            //
            //     // 2️⃣ Синхронизация серверного времени
            //     // Первая синхронизация
            //     yield return _coroutines.StartCoroutine(ServerTimeSync.Instance.SyncServerTime());
            //
            //     // 3️⃣ Инициализация игровых систем (оставил здесь как пример)
            //     //WorkshopTimerSystem.Instance.Initialize();
            //     //WorldEventSystem.Instance.Initialize();
            //
            //     // 4️⃣ Загружаем данные игрока с сервера
            //     //yield return _coroutines.StartCoroutine(WorkshopTimerSystem.I.LoadPlayerTasks());
            //
            //     // 5️⃣ Стартуем корутину пинга сервера
            //     _coroutines.StartCoroutine(ServerConnectionChecker.I.CheckRoutine());
            //
            //     DLog.Alert("✅ Cерверное время синхронизировано!");
            // }

            // ****************************************************************************************************
            // ****************************************************************************************************
            // ****************************************************************************************************
            
            
            
            // в _rootContainer так же можно положить сервuсы состояния, аналитики, monetisation, 
            // т.е все что используется во всей игре
            
            
            
            // === server service
            var serverAPI = new ServerAPI();
            _rootContainer.RegisterInstance<IServerAPI>(serverAPI);
            
            var serverTimeSync = new ServerTimeSync(_rootContainer, _coroutines);
            _rootContainer.RegisterInstance<IServerTimeSync>(serverTimeSync);
            
            var connectionChecker = new ServerConnectionChecker(serverAPI, _coroutines);
            _rootContainer.RegisterInstance<IServerConnectionChecker>(connectionChecker);
            connectionChecker.OnConnectionChanged += connected =>
            {
                if (!connected)
                    DLog.Alert("⚠️ Сервер недоступен! Синхронизация приостановлена", EDlogColor.RED);
                else
                    DLog.Alert("✅ Сервер снова доступен! Синхронизация восстановлена");
            };
            

            // === загружаем настройки игры и ассеты
            var configProvider = new ConfigProvider();
            _rootContainer.RegisterInstance<IConfigProvider>(configProvider);
            yield return _rootContainer.Resolve<IConfigProvider>().LoadAllConfigs();
            
            // === регистрируем игровой контент
            GameContent.Initialize(configProvider);
            GameIdProvider.Initialize(configProvider.Get<GameIds>());
            
            
            // === save service 
            var confingsStateProvider = new PlayerPrefsConfingsStateProvider();
            confingsStateProvider.LoadGameSettings();    // выгружаем настройки игры
            _rootContainer.RegisterInstance<IGameSettingsStateProvider>(confingsStateProvider);
            _rootContainer.RegisterInstance<IGameStateProvider>(new JsonGameStateProvider(configProvider));

            // === scene sercvice (для переключения сцен)
            _rootContainer.RegisterInstance(new SceneService(_coroutines));
            _rootContainer.RegisterInstance(new LocationTransitionService((index, locationEntry) =>
            {
                SystemRepository.CampDefense = locationEntry.CampDefense;
                switch (index)
                {
                    case -1: // map
                        _coroutines.StartCoroutine(LoadAndStartWorldMap(new WorldMapEnterParams(0)));
                        break;

                    case 0: // home
                        if (!locationEntry.CampDefense)
                        {
                            var enterParams = new CampEnterParams(0);
                            enterParams.ResetRootPlayerScene = locationEntry.ResetRootPlayerScene;
                            _coroutines.StartCoroutine(LoadAndStartHome(enterParams));
                        }
                        else
                            _coroutines.StartCoroutine(LoadAndStartLocation(new LocationEnterParams(0)));
                        break;

                    default: // locations
                        _coroutines.StartCoroutine(LoadAndStartLocation(new LocationEnterParams(0)));
                        break;
                }
            }));
            
            
            // загрузка сторонних сдк 
            _rootContainer.RegisterInstance(new SDKStarter(_coroutines));
            
            // ****************************************************************************************************
            // ****************************************************************************************************
            
            
            
            _instance.StartGame();
        }


        void StartGame()
        {
#if UNITY_EDITOR
            var sceneName = SceneManager.GetActiveScene().name;

            // if (sceneName == Scenes.CORE)
            // {
            //     _coroutines.StartCoroutine(LoadAndStartCore());
            //     return;
            // }
            //
            
            // *** на случай если активна сцена из какого-то packeges (только для редактора)
            // что бы игра со своими сценами не запускалась
            if (sceneName != Scenes.BOOT)
            {
                DLog.Alert("Основная сцена не BOOT, загрузка остановлена", EDlogColor.RED);
                return;
            }
#endif

            _coroutines.StartCoroutine(LoadAndStartCore());
        }

        // запуск основной сцены с контроллерами
        IEnumerator LoadAndStartCore()         
        {
            _uiRoot.ShowLoadingScreen();
            _cashedSceneContainer?.Dispose();
            // ***************************************************************************************************
            
            
            // ***************************************************************************************************
            // процесс загрузки

            // сначало загружаем пустую сцену
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.BOOT))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.BOOT, false);
            
            // потом загружаем сцену контроллер 
            yield return _coroutines.StartCoroutine(LoadAndStartGameplay());
            
            
            yield return null;

            // процесс загрузки
            // ***************************************************************************************************
            // ***************************************************************************************************
            // ***************************************************************************************************
            // ! ПОСЛЕ ЗАГРУЗКИ CORE СЦЕНЫ ЗАГРУЖАЕМ СЕРВИСЫ ПРИВЯЗАННЫЕ К НЕЙ ! 
            
            var configProvider = _rootContainer.Resolve<IConfigProvider>();
            var gameStateProvider = _rootContainer.Resolve<IGameStateProvider>();
            var entryPointConfig = configProvider.Get<ApplicationConfig>();
            
            LoadingManagerSteps.CreateStepList(_rootContainer);

            
            // *** создаем голобальные объекты которые не будут уничтожатся при смене сцен **************************
            ScreenProfiler.Init(entryPointConfig.showScreenLogs);
            
            // кнопка выхода на панели ошибки сервера
            //serverError.transform.Find("b_server_error").gameObject.EventBtn(Application.Quit);
            
            // для поиска не активных объектов, сначало находим корень
            // и потом ищем нужный объект
            Transform root = GameObject.Find("Canvas Core").transform;
            
            //CORT.blockScreen = root.Find("BLOCK").gameObject;
            //CORT.BlockScreen(false);
            //CORT.loadPay = root.Find("LOAD_PAY").gameObject;
            //CORT.LoadPay(false);
            
            
            // инициализируем сервис локатор
            ServiceLocator.Initiailze();
            Object.FindFirstObjectByType<CoreServiceLocatorAssembler>().Initialize(_rootContainer);
            
            // внешний доступ для смены сцен
            ServiceLocator.Current.Register(_rootContainer.Resolve<LocationTransitionService>());
            
            StyleManager.Initialize();
            
            
            // #1 load language
            ServiceLocator.Current.Get<LocalisationService>().LoadLanguage();
            // ***************************************************************************************************
            
            // #2 делаем запросы разрешений от пользователя
            if (entryPointConfig.isAppstore)
            {
                LoadingManager.I.NewStepStarted(CServiceType.PLAYER_PERMISSION);
                yield return new Config().CheckRequest(_coroutines, configProvider.Get<GameConfig>().Ios);
            }
            // ***************************************************************************************************
            
            // #3 выгружаем состояние из сохранения
            var isGameStateLoaded = false;
            _rootContainer.Resolve<IGameStateProvider>().LoadGameState().Subscribe(_ => isGameStateLoaded = true);
            yield return new WaitUntil(() => isGameStateLoaded);
            // ***************************************************************************************************
            
            
            // >>>  регистрация глобальных сервисов  <<<
            LoadingManager.I.NewStepStarted(CServiceType.REGISTER_GLOBAL_SERVICES);
            CoreRegistrations.Register(_rootContainer, _coroutines);
            
            
            
            
            // ***************************************************************************************************
            // >>>      SDK     <<<
            
            
            // === sdk analitics
            if (entryPointConfig.requiresAnalyticsService)
            {
                LoadingManager.I.NewStepStarted(CServiceType.ANALYTICS);
                DLog.Alert("Start: load analitics", EDlogColor.YELLOW, AppConstants.show_log_core);
                // SetProcessText("Init");
                // SetProgress(5);
                FBA.Init();
                yield return new WaitForSeconds(1);
            }
            
            // === sdk IAP 
            if (entryPointConfig.requiresIapService)
            {
                LoadingManager.I.NewStepStarted(CServiceType.IAP);
                DLog.Alert("Start: load IAP", EDlogColor.YELLOW, AppConstants.show_log_core);
                // SetProcessText("Load M-I");
                // SetProgress(9);
                yield return _coroutines.StartCoroutine(_rootContainer.Resolve<SDKStarter>()
                    .InitializeSDK(_rootContainer.Resolve<GameStoreService>()));
            }
            
            // === sdk Ad
            if (entryPointConfig.requiresAdService)
                yield return _coroutines.StartCoroutine(InitializeAdService());
            //else
                //ServiceLocator.Current.Get<AdController>().Inactive();
            // if (entryPointConfig.requiresAdService)
            // {
            //     
            //     // *** запрос на согласие на персонализированую рекламу ******************************************
            //     bool waitConsent = true;
            //     var _consentController = new ConsentController();
            //     
            //     // #1 кнопка для показа формы (сделано в SettingsUI)
            //     // ServiceLocator.Current.Get<W_Options>().buttonsFeature.bConsentOption.EventBtnOne(() => 
            //     //     _consentController.ShowPrivacyOptionsForm(error =>
            //     //     {
            //     //         if (error != null)
            //     //         {
            //     //             ScreenProfiler.AddMessage($"GDPR : Failed to show privacy option form >> {error}".SetText(EDlogColor.ORANGE));
            //     //         }
            //     //     }));
            //     
            //     // #2 запрос/загрузка согласия
            //     _consentController.GatherConsent(
            //         configProvider.Get<GameConfig>(),
            //         (error, canRequestAds) =>
            //         {
            //             if (error != null)
            //             {
            //                 ScreenProfiler.AddMessage($"GDPR : {error}".SetText(EDlogColor.ORANGE));
            //             }
            //
            //             // *** по идее запуск сдк рекламы должен происходить только при наличии согласия
            //             if (canRequestAds)
            //             {
            //                 ScreenProfiler.AddMessage("GDPR : Can request Ads");
            //                 // ServiceLocator.Current.Get<ADSController>().Init()
            //                 // оставил старый запуск сдк рекламы, иначе реклама не будет работать в игре
            //             }
            //
            //             waitConsent = false;
            //         });
            //     while (waitConsent) yield return null;
            //     // ***********************************************************************************************
            //
            //
            //     DLog.Alert("Start: load ADS", EDlogColor.YELLOW, AppConstants.show_log_core);
            //     //SetProcessText("Load M-II");
            //     //SetProgress(48);
            //     
            //     LoadingManager.I.NewStepStarted(CServiceType.AD);
            //     yield return _coroutines.StartCoroutine(ServiceLocator.Current.Get<AdController>().Init());
            // }
            // else
            // {
            //     ServiceLocator.Current.Get<AdController>().Inactive();
            // }
            
            // ===== вызываем единственное событие при загрузки приложения =====
            EventBus<LoadAndStartCoreEvent>.Raise(new LoadAndStartCoreEvent());

#if UNITY_EDITOR
            yield return null;
#else
            yield return new WaitForSeconds(1);
#endif
            
            
            if (AppConstants.SERVER_ON)
            {
                // 1️⃣ Проверяем соединение с сервером
                bool connected = false;
                yield return _coroutines.StartCoroutine(_rootContainer.Resolve<IServerAPI>()
                    .PingServer((ok) => connected = ok));

                if (!connected)
                {
                    DLog.Alert("Нет соединения с сервером!", EDlogColor.RED);
                    yield break;
                }

                // 2️⃣ Синхронизация серверного времени
                // Первая синхронизация
                yield return _coroutines.StartCoroutine(_rootContainer.Resolve<IServerTimeSync>().SyncServerTime());

                // 3️⃣ Инициализация игровых систем (оставил здесь как пример)
                //WorkshopTimerSystem.Instance.Initialize();
                //WorldEventSystem.Instance.Initialize();

                // 4️⃣ Загружаем данные игрока с сервера
                //yield return _coroutines.StartCoroutine(WorkshopTimerSystem.I.LoadPlayerTasks());

                // 5️⃣ Стартуем корутину пинга сервера
                _coroutines.StartCoroutine(_rootContainer.Resolve<IServerConnectionChecker>().CheckRoutine());

                DLog.Alert("✅ Cерверное время синхронизировано!");
            }
            
            
            DLog.Alert($"[All services loaded : {Time.time}] ", EDlogColor.BLUE, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            // ***************************************************************************************************
            // >>>      SCENE GAMEPLAY     <<<   // дальше пошла игра
            
            
            
            
            // запускаем аналитику
            AnalyticsService.SetAnalyticsMode(entryPointConfig.requiresAnalyticsService);
            AnalyticsService.Gameplay(AnalyticsService.ERequestGameplay.Start_App);
            
            
            // проверяем статус игрока
            //ServiceLocator.Current.Get<DeathSystem>().InitializePlayerState(_rootContainer);
            
            
            // === для первого старта игры ===
            new NewGameEntry();
            // =====================================================================================================
            
            
            // ====     загружаем основную сцену    =====
            LoadingManager.I.NewStepStarted(CServiceType.LOADING_MAIN_SCENE);
            switch (entryPointConfig.startAppScene)
            {
                // **** в релизной версии должны загружать сцену из сохраненного состояния игры
                case ApplicationConfig.EStartApp.RELEASE:
                {
                    
                    if (_rootContainer.Resolve<IGameStateProvider>().GameStateProxy.GameLoopContext.PlayerOnMap.CurrentValue ||
                        gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value > 0)
                    {
                        // если игрок был на карте или в локации всегда загружаем карту
                        // (локация никогда не загружается)
                        yield return _coroutines.StartCoroutine(LoadAndStartWorldMap(new WorldMapEnterParams(0)));
                    }
                    else // if location == 0
                    {
                        yield return _coroutines.StartCoroutine(LoadAndStartHome(new CampEnterParams(0)));
                    }
                } break;
                // ***********************************************************************************************
                
                
                // ! загрузка сцен для режима разработки !
                case ApplicationConfig.EStartApp.Home:
                {
                    gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value = 0;
                    yield return _coroutines.StartCoroutine(LoadAndStartHome(new CampEnterParams(0)));

                } break;
                
                case ApplicationConfig.EStartApp.Map:
                {
                    gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value = -1;
                    yield return _coroutines.StartCoroutine(LoadAndStartWorldMap(new WorldMapEnterParams(0)));

                } break;
                
                case ApplicationConfig.EStartApp.Location:
                {
                    gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value =
                        _rootContainer.Resolve<IConfigProvider>().Get<ApplicationConfig>().startingLocationId;
                    yield return _coroutines.StartCoroutine(LoadAndStartLocation(new LocationEnterParams(0)));

                } break;
                
                case ApplicationConfig.EStartApp.DevScene:
                {
                    gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value = 1;
                    yield return _coroutines.StartCoroutine(LoadAndStartDevScene(new DevSceneEnterParams(0)));

                } break;
            }
            // ***************************************************************************************************
            // ***************************************************************************************************
            // ***************************************************************************************************
            // ***********      все необходимые сцены готовы, загружаем зависимости     **************************

            
            // === show bar 100% === 
            LoadingManager.I.Complete();
            yield return new WaitForSeconds(.5f);
            //
            
            
            //AudioService.PlayMusic("Main");
            
            
            //var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
            //var gameplayContainer = _cashedSceneContainer = new DIContainer(_rootContainer);
            //sceneEntryPoint.Run(gameplayContainer);


            
            _GameState.AppLoaded_();
            // ***************************************************************************************************
            _uiRoot.HideLoadingScreen();
            DLog.Alert($"[Game started : {Time.time}]", EDlogColor.YELLOW);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
        }



        // техническая сцена
        IEnumerator LoadAndStartGameplay()
        {
            yield return null;
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.CORE_GAMEPLAY))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.CORE_GAMEPLAY, false);
            
            
            var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
            sceneEntryPoint.Run(_rootContainer);
            
        }
        
        
        
        /*
         *      Для каждой сцены нужен свой метод IEnumerator LoadAndStart...
         * 
         *      т.е если появляется идея сцены с новыми механиками (расширение игры в релизе)
         *      под эту новую сцену создаем новый метод IEnumerator LoadAndStart который отвечает только за нее !!!
         *      там настраиваем нужные зависимости и состояния для сцены не опасаясь за сущ. методы для других сцен
         *
         *      >> таким образом для загрузки любой сцены из любого состояния нужно всего лишь вызвать
         *          метод LoadAndStart... закрепленный за сценой и дальше он все сделает
         */
        
        IEnumerator LoadAndStartWorldMap(WorldMapEnterParams worldMapEnterParams = null)                    // MAP
        {
            AudioService.StopMusic();
            _uiRoot.ShowLocationLoadScreen(new LocationLoadingScreen.LocationLoadDTO()
            {
                locationName = "Global Map"
            });
            
            // ждем очистки после предыдущей сцены
            ClearSubscriptions();
            // ***************************************************************************************************
            
            
            
            // ***************************************************************************************************
            // процесс загрузки

            // сначало загружаем пустую сцену
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.BOOT))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.BOOT, false);
            
            yield return null;

            // процесс загрузки
            // ***************************************************************************************************

            DLog.Alert("Load scene : load scene", EDlogColor.YELLOW, AppConstants.show_log_core);
            // загружаем основную сцену
            yield return _rootContainer.Resolve<SceneService>().LoadScene(Scenes.MAP, true);
            // ***************************************************************************************************
            // ***********      все необходимые сцены готовы, загружаем зависимости     **************************
            
            
            DLog.Alert("Load scene : load services", EDlogColor.YELLOW, AppConstants.show_log_core);
            var sceneEntryPoint = Object.FindFirstObjectByType<WorldMapEntryPoint>();
            _cashedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(_cashedSceneContainer, worldMapEnterParams).Subscribe(mapExitParams =>
            {
                var targetSceneName = mapExitParams.TargetSceneEnterParams.SceneName;

                switch (targetSceneName)
                {
                    case Scenes.HOME:
                        _coroutines.StartCoroutine(LoadAndStartHome(mapExitParams.TargetSceneEnterParams.As<CampEnterParams>()));
                        break;
                    
                    case Scenes.LOCATION:
                        _coroutines.StartCoroutine(LoadAndStartLocation(mapExitParams.TargetSceneEnterParams.As<LocationEnterParams>()));
                        break;
                }
            });
            
            
            // ***************************************************************************************************
            // ***********      все сервисы загружены     ********************************************************

            yield return new WaitForSeconds(1); // поставил костыль для ожидания, что бы портреты юнитов успели делаться
            yield return sceneEntryPoint.Initialize(_cashedSceneContainer);
            
            
            DLog.Alert("Load scene : activate screen manager", EDlogColor.YELLOW, AppConstants.show_log_core);
            // ***********           запускаем экраны            *************************************************
            // ...
            
            
            
            
            
            // включаем звуки юнитов
            AudioService.PlayMusic("Main");
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
            // ***************************************************************************************************
            _uiRoot.HideLocationLoadScreen();
            DLog.Alert($"[Scene map completed : {Time.time}]", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
        
        
        
        
        
        IEnumerator LoadAndStartHome(CampEnterParams campEnterParams)                                   // CAMP
        {
            AudioService.StopMusic();
            _uiRoot.ShowLocationLoadScreen(new LocationLoadingScreen.LocationLoadDTO()
            {
                locationName = "Your Camp"
            });
            
            
            // ждем очистки после предыдущей сцены
            ClearSubscriptions();

            // для сброса подписок и пр
            if (campEnterParams.ResetRootPlayerScene)
                yield return _rootContainer.Resolve<SceneService>().UnloadScene(Scenes.ROOT_PLAYER);
            // ***************************************************************************************************

            
            
            
            // ***************************************************************************************************
            // процесс загрузки
            
            // сначало загружаем пустую сцену
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.BOOT))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.BOOT, false);
            
            
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.ROOT_PLAYER))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.ROOT_PLAYER, false);
            
            yield return null;

            // процесс загрузки
            // ***************************************************************************************************

            DLog.Alert("Load scene : load scene", EDlogColor.YELLOW, AppConstants.show_log_core);
            // загружаем основную сцену
            yield return _rootContainer.Resolve<SceneService>().LoadScene(Scenes.HOME, true);
            // ***************************************************************************************************
            // ***********      все необходимые сцены готовы, загружаем зависимости     **************************
            
            
            DLog.Alert("Load scene : load services", EDlogColor.YELLOW, AppConstants.show_log_core);
            var sceneEntryPoint = Object.FindFirstObjectByType<CampEntryPoint>();
            _cashedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(_cashedSceneContainer, campEnterParams).Subscribe(campExitParams =>
            {
                _coroutines.StartCoroutine(LoadAndStartWorldMap(campExitParams.WorldMapEnterParams));
            });
            

            
            // ***************************************************************************************************
            // ***********      все сервисы загружены     ********************************************************
            
            yield return new WaitForSeconds(1); // поставил костыль для ожидания, что бы портреты юнитов успели делаться
            yield return sceneEntryPoint.Initialize(_cashedSceneContainer);
            
            
            
            DLog.Alert("Load scene : activate screen manager", EDlogColor.YELLOW, AppConstants.show_log_core);
            // ***********           запускаем экраны            *************************************************
            // ...
            
            
            
            
            
            
            // включаем звуки юнитов
            AudioService.PlayMusic("Main");
            EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
            // ***************************************************************************************************
            _uiRoot.HideLocationLoadScreen();
            DLog.Alert($"[Scene camp completed : {Time.time}]", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
        
        
        IEnumerator LoadAndStartLocation(LocationEnterParams locationEnterParams)                   // LOCATION
        {
            AudioService.StopMusic();
            
            var configProvider = _rootContainer.Resolve<IConfigProvider>();
            var gameStateProvider = _rootContainer.Resolve<IGameStateProvider>();

            var locationConfig = configProvider.Get<LocationsConfigs>()
                .Locations[gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value];
            
            _uiRoot.ShowLocationLoadScreen(new LocationLoadingScreen.LocationLoadDTO()
            {
                locationName = locationConfig.Header.TitleLid
            });
            
            // ждем очистки после предыдущей сцены
            ClearSubscriptions();
            // ***************************************************************************************************
            
            
            
            // ***************************************************************************************************
            // процесс загрузки

            // сначало загружаем пустую сцену
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.BOOT))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.BOOT, false);
            
            
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.ROOT_PLAYER))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.ROOT_PLAYER, false);
            
            yield return null;

            // процесс загрузки
            // ***************************************************************************************************

            DLog.Alert("Load scene : load scene", EDlogColor.YELLOW, AppConstants.show_log_core);
            // загружаем основную сцену
            yield return _rootContainer.Resolve<SceneService>().LoadScene(Scenes.LOCATION, true);
            // ***************************************************************************************************
            // ***********      все необходимые сцены готовы, загружаем зависимости     **************************
            
            
            DLog.Alert("Load scene : load services", EDlogColor.YELLOW, AppConstants.show_log_core);
            var sceneEntryPoint = Object.FindFirstObjectByType<LocationEntryPoint>();
            _cashedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(_cashedSceneContainer, locationEnterParams).Subscribe(locationExitParams =>
            {
                _coroutines.StartCoroutine(LoadAndStartWorldMap(locationExitParams.WorldMapEnterParams));
            });

            
            // ***************************************************************************************************
            // ***********      все сервисы загружены     ********************************************************
            
            yield return new WaitForSeconds(1); // поставил костыль для ожидания, что бы портреты юнитов успели делаться
            yield return sceneEntryPoint.Initialize(_cashedSceneContainer);
            yield return new WaitForSeconds(1.5f); // что бы сетка A* успела правильно создаться
            
            
            DLog.Alert("Load scene : activate screen manager", EDlogColor.YELLOW, AppConstants.show_log_core);
            // ***********           запускаем экраны            *************************************************
            // ...
            
            
            
            
            
            
            // включаем звуки юнитов
            AudioService.PlayMusic("Combat");
            EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
            // ***************************************************************************************************
            _uiRoot.HideLocationLoadScreen();
            DLog.Alert($"[Scene location completed : {Time.time}]", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
        
        
        IEnumerator LoadAndStartDevScene(DevSceneEnterParams devSceneEnterParams)                   // DEV SCENE
        {
            AudioService.StopMusic();
            
            var configProvider = _rootContainer.Resolve<IConfigProvider>();
            var gameStateProvider = _rootContainer.Resolve<IGameStateProvider>();

            var locationConfig = configProvider.Get<LocationsConfigs>()
                .Locations[gameStateProvider.GameStateProxy.GameLoopContext.CurrentLocationStateId.Value];
            
            _uiRoot.ShowLocationLoadScreen(new LocationLoadingScreen.LocationLoadDTO()
            {
                locationName = locationConfig.Header.TitleLid
            });
            
            // ждем очистки после предыдущей сцены
            ClearSubscriptions();
            // ***************************************************************************************************
            
            
            
            // ***************************************************************************************************
            // процесс загрузки

            // сначало загружаем пустую сцену
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.BOOT))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.BOOT, false);
            
            
            if (!_rootContainer.Resolve<SceneService>().SceneExist(Scenes.ROOT_PLAYER))
                yield return _rootContainer.Resolve<SceneService>().LoadSceneAsync(Scenes.ROOT_PLAYER, false);
            
            yield return null;

            // процесс загрузки
            // ***************************************************************************************************

            DLog.Alert("Load scene : load scene", EDlogColor.YELLOW, AppConstants.show_log_core);
            // загружаем основную сцену
            yield return _rootContainer.Resolve<SceneService>().LoadScene(Scenes.DEV_SCENE, true);
            // ***************************************************************************************************
            // ***********      все необходимые сцены готовы, загружаем зависимости     **************************
            
            
            DLog.Alert("Load scene : load services", EDlogColor.YELLOW, AppConstants.show_log_core);
            var sceneEntryPoint = Object.FindFirstObjectByType<DevSceneEntryPoint>();
            _cashedSceneContainer = new DIContainer(_rootContainer);
            sceneEntryPoint.Run(_cashedSceneContainer, devSceneEnterParams).Subscribe(exitParams =>
            {
                _coroutines.StartCoroutine(LoadAndStartWorldMap(exitParams.WorldMapEnterParams));
            });

            
            // ***************************************************************************************************
            // ***********      все сервисы загружены     ********************************************************
            
            yield return sceneEntryPoint.Initialize(_cashedSceneContainer);
            
            
            
            DLog.Alert("Load scene : activate screen manager", EDlogColor.YELLOW, AppConstants.show_log_core);
            // ***********           запускаем экраны            *************************************************
            // ...
            
            
            
            
            
            
            // включаем звуки юнитов
            AudioService.PlayMusic("Combat");
            EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
            // ***************************************************************************************************
            _uiRoot.HideLocationLoadScreen();
            DLog.Alert($"[Scene DEV completed : {Time.time}]", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
            DLog.Alert("********************************************************************", EDlogColor.YELLOW, AppConstants.show_log_core);
        }




        IEnumerator InitializeAdService()
        {
            var configProvider = _rootContainer.Resolve<IConfigProvider>();
            
            var consentService = new ConsentService();
            ServiceLocator.Current.Register(consentService);

            bool consentDone = false;

            consentService.GatherConsent(configProvider.Get<GameConfig>(), () =>
            {
                consentDone = true;
            });

            while (!consentDone)
                yield return null;
            
            // *** по идее запуск сдк рекламы должен происходить только при наличии согласия
            //if (consentService.CanRequestAds) // скрыто, иначе реклама не будет работать в игре
            {
                //ScreenProfiler.AddMessage("GDPR : Can request Ads");
                
                // --- SDK INIT (инфраструктура)
                LoadingManager.I.NewStepStarted(CServiceType.AD);
                var sdk = new AdMobInitializer();
                var task = sdk.InitializeAsync();
                
                while (!task.IsCompleted) 
                    yield return null;
                
                // sdk запущено
                // создаем прелоад и запускаем сервис
                var adapter = new AdMobAdapter();
                var preload = new AdPreloadService(
                    adapter,
                    sdk,
                    _coroutines); 

                preload.Start();

                var adService = AdInstaller.Create(_rootContainer, _coroutines, adapter, preload);
                ServiceLocator.Current.Register(adService);
            }
        }

        




        void ClearSubscriptions()
        {
            DLog.Alert("Load scene : clear cashed services", EDlogColor.YELLOW, AppConstants.show_log_core);
            
            // ! не менять порядок !
            _cashedSceneContainer?.Dispose();
            EventBus<SceneClearEvent>.Raise(new SceneClearEvent());
            EventBus<SceneServicesClearEvent>.Raise(new SceneServicesClearEvent());
            EventBus<SceneServicesResetReusableEvent>.Raise(new SceneServicesResetReusableEvent());
            //
            EventBus<SceneClearEvent>.Clear();
            EventBus<SceneServicesClearEvent>.Clear();
            // ! не менять порядок !
        }
    }
}