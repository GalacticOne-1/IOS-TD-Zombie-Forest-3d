using System;
using System.Collections;
using Galactic1.Mobile;
using Galactic1;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1
{
    // старт приложения
    // загрузка нужных сцен и активов
    public class ApplicationSetup : Singleton<ApplicationSetup>
    {
        [SerializeField] private CoreServiceLocatorAssembler coreServiceLocatorAssembler;
        public GameObject coreCanvas;

        public bool ON_EDITOR;

        #region EDITOR

        public int tab;

        #endregion
        
        
        public TextMeshProUGUI processTitle, 
            tProgress,
            version;
        public Image progressBar;
        public GameObject logo;            // заставка при запуске приложения
        public GameObject logoBox;
        public bool USE_LOGO;
        public float durationLoad = 3;     // продолжительность загрузки между сценами
        
        // для какого магазина релиз
        public bool APPSTORE;
        //public EStartApp startApp;
        public bool MODE_REGULAR;
        [Header("false - каждый запуск как в первый раз")]
        public bool USE_SAVING;
        [Header("false - без получения настроек и времени с сервера")]
        public bool USE_SERVER;
        public bool USE_REVIEW;
        public bool USE_INTRO;
        public bool USE_IAP;
        public bool CANCEL_LOAD;

        
        // public EPlayerController PLAYER_CNTR;
        //
        // public enum EPlayerController
        // {
        //     NON, MOBILE, KEYBOARD
        // }
        
        
        
        // ожидание подключения
        [Header("* SERVER")] 
        public GameObject serverConnect;
        public GameObject serverError;
        public float tryConnectWait = 1f;
        public bool connectLog = true;


        [Space] 
        public bool USE_ANALITICS;
        public bool USE_ADS;
        [Header("Для релиза снять!")] public bool AD_TEST;
        [Header("true - что бы работало!")] public bool TUTORIAL;


        public bool SCREEN_LOG;
        public bool BTN_CRASH;
        
        
        #region URL

        string url_time_server = "https://galactic1games.com/go.php";
        public string Url_time_server => url_time_server;
        
        
        
        #endregion




        #region START APP 
        
        //public static bool FIRST_LAUNCH;
        //public static bool APP_LOAD = false;                  // приложение загружено

        public Action gameplayComplete, lobbyComplete;
        
        #endregion

        #region Progress load app

        private float currentProgress;
        
        // полоса готовности игры
        public void SetProgress(float newCurr)
        {
            tProgress.text = $"{newCurr}%";
            progressBar.fillAmount = newCurr / 100.0f;
        }
        
        /// <summary>
        /// Для указания названия процесса 
        /// </summary>
        /// <param name="t"></param>
        public void SetProcessText(string t)
        {
            processTitle.text = t;
        }


        
        #endregion
        
        
        
        
        
        
        
        
        
        
        
        
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        

        // приложение начинается здесь, с единственной сцены CORE
        // которая запускает все нужное
        private void Start()
        {
            return;
            // панель для блокировка экрана до готовности приложения
            //if (!USE_LOGO)
                //FindObjectOfType<AppLoading>(true).gameObject.SetActive(true);
            
            //new ANALITICS_FOR_GAMEPLAY(ANALITICS_FOR_GAMEPLAY.ERequestGameplay.Start_App);
            
            
            Application.targetFrameRate = 30;
            
            
            // *** 1
            ServiceLocator.Initiailze();
            //coreServiceLocatorAssembler.Initialize();
            // ***
            
            
            // >>>
            ResourceRequest request = Resources.LoadAsync("Build", typeof(BuildScriptableObject));
            request.completed += Request_completed;
            
            
            /*if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }*/
            ScreenProfiler.Init(SCREEN_LOG);
            //ScreenSizeController.SetScreenSize();
            Input.multiTouchEnabled = true;  

            
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = true;
            
            // * кнопка выхода на панели ошибки сервера
            serverError.transform.Find("b_server_error").gameObject.EventBtn_old(Application.Quit);
            
            
            // для поиска не активных объектов, сначало находим корень
            // и потом ищем нужный объект
            //Transform root = GameObject.Find("Canvas Core").transform;
            
            //CORT.blockScreen = root.Find("BLOCK").gameObject;
            //CORT.BlockScreen(false);
            //CORT.loadPay = root.Find("LOAD_PAY").gameObject;
            //CORT.LoadPay(false);

            
#if  UNITY_EDITOR
            tryConnectWait = .4f;
            durationLoad = .4f;
#endif

            DLog.Alert(">>> DEV MANAGER");
            DeveloperConsole.I.Activator();

            
            // *****************    DATA LOADING    ****************************************************************
            //DLog.Alert("LOADING");
            //VariantStartApp();
            LoadData();
            // *****************************************************************************************************
            
            
            // при старте приложения, загружаем язык
            // это событие вызывается после готовности языка
            DataSaver.I.onContinue = data =>
            {
                // производим загрузку данных
                // и только после этого продолжаем 
                //ServiceLocator.Current.Get<LocalisationController>().Data = DataSaver.I.ConvertData<CLoc>(data);
                StartCoroutine(logo_box());
                StartCoroutine(Launch());
            };
            
            // запуск загрузки языка
            DLog.Alert("Start: load language", EDlogColor.YELLOW, SCREEN_LOG);
            SetProcessText("Load language");
            //ServiceLocator.Current.Get<LocalisationController>().LoadLocalization(ServiceLocator.Current.Get<W_Options>().GetLangKey());
        }

        public float smooth;
        IEnumerator logo_box()
        {
            Vector2 v = logoBox.transform.localPosition;
            DLog.Alert($"*** {logoBox.transform.localPosition}_{v}");
            //while (!APP_LOAD && logoBox.transform.localPosition.x < 250)
            {
                yield return null;
                
                v.x = Mathf.MoveTowards(v.x, 250, smooth * Time.deltaTime);
                logoBox.transform.localPosition = v;
            }
        }
        
        private void Request_completed(AsyncOperation obj)
        {
            BuildScriptableObject buildScriptableObject = ((ResourceRequest)obj).asset as BuildScriptableObject;

            if (buildScriptableObject == null)
            {
                Debug.LogError("Build scriptable object not found in resources directory! Check build log for errors!");
            }
            else
            {
                version.text = $"Version {Application.version}.{buildScriptableObject.BuildNumber}";
                //ServiceLocator.Current.Get<W_Options>().version.text = version.text;
            }
        }

        
        IEnumerator Launch()
        {
            yield return null;
            yield return null;
            
            
            if (CANCEL_LOAD) yield break;
            
            // язык и сохранение загружены
            // получаем настройки из сервера
            // if(USE_SERVER)
            // {
            //     for (int i = 0; i < 10; i++)
            //     {
            //         DLog.Alert($"REMOTE CONFIG TRY CONNECT {i}", EDlogColor.YELLOW, SCREEN_LOG);
            //         // 1 делаем запрос
            //         var corSerever = StartCoroutine(ServiceLocator.Current.Get<ConfigsProvider>().LoadGameSettings());
            //
            //         // #2 ждем
            //         for (float w = tryConnectWait; w >= 0; w -= Time.deltaTime)
            //         {
            //             // #3 проверяем подключение
            //             if (ServiceLocator.Current.Get<ConfigsProvider>().Complete)
            //             {
            //                 break;
            //             }
            //             
            //             yield return null;
            //         }
            //
            //         if (ServiceLocator.Current.Get<ConfigsProvider>().Complete)
            //             break;
            //
            //         StopCoroutine(corSerever);
            //     }
            //
            //     if (!ServiceLocator.Current.Get<ConfigsProvider>().Complete)
            //     {
            //         new SERVER_ConnectError();
            //         yield break;
            //     }
            // }
            
            // *** делаем запросы разрешений от пользователя
            //if (APPSTORE)
            {
                //yield return new Config().CheckRequest(this, ServiceLocator.Current.Get<SettingsProvider>().GameSettings.Ios);
            }
            // --------------- 

            
            // if (USE_ANALITICS)
            // {
            //     DLog.Alert("Start: load analitics", EDlogColor.YELLOW, SCREEN_LOG);
            //     SetProcessText("Init");
            //     SetProgress(5);
            //     FBA.Init();
            //     yield return new WaitForSeconds(1);
            // }
            
            // if (USE_IAP)
            // {
            //     DLog.Alert("Start: load IAP", EDlogColor.YELLOW, SCREEN_LOG);
            //     SetProcessText("Load M-I");
            //     SetProgress(9);
            //     //yield return StartCoroutine(SDKStarter.I.IAP());
            // }
            
            
            // *****************    DATA SETUP    ****************************************************************
            NewData();
            // ***************************************************************************************************
            
            
            // Выгружаем локальные хар-ки из JSON
            //new FeatureLoader();
            
            // ***      INITIALIZE ASSETS
            
            // лучше ассеты иницилизировать перед созданием сцены
            // (не трогать иначе сохранение не бует работать для ассетов!!!)
            
            //ServiceLocator.Current.Get<LibController>().furnituresSettings.Init();
            //ServiceLocator.Current.Get<LibController>().Init();
           
            
            yield return null;
            yield return null;
            // ***
            
            
            // ***       первым делом загружаем дату
            //VariantStartApp();
#if  UNITY_EDITOR
            yield return new WaitForSeconds(MODE_REGULAR ? 1 : .2f);
#else
            yield return new WaitForSeconds(1);
#endif
            
            // ServiceLocator.Current.Get<MonoBehaviourMaster>().Activator();
            // ServiceLocator.Current.Get<AudioController>().Activator(); 
            // ServiceLocator.Current.Get<MusicManagement>().Activator();
            // ServiceLocator.Current.Get<W_Options>().Activator();
            
            SetProgress(30);
            //while (process > 0) yield return null;
            
            
            //gameplayComplete = CompleteGameplay;
            lobbyComplete = CompleteLobby;

            // if (USE_ADS)
            // {
            //     
            //     // *** запрос на согласие на персонализированую рекламу ******************************************
            //     bool waitConsent = true;
            //     var _consentController = new ConsentController();
            //     
            //     // #1 кнопка для показа формы
            //     ServiceLocator.Current.Get<W_Options>().buttonsFeature.bConsentOption.EventBtnOne(() => 
            //         _consentController.ShowPrivacyOptionsForm(error =>
            //         {
            //             if (error != null)
            //             {
            //                 ScreenProfiler.AddMessage($"GDPR : Failed to show privacy option form >> {error}".SetText(EDlogColor.ORANGE));
            //             }
            //         }));
            //     
            //     // #2 запрос/загрузка согласия
            //     // _consentController.GatherConsent((error, canRequestAds) =>
            //     // {
            //     //     if (error != null)
            //     //     {
            //     //         ScreenProfiler.AddMessage($"GDPR : {error}".SetText(EDlogColor.ORANGE));
            //     //     }
            //     //
            //     //     // *** по идее запуск сдк рекламы должен происходить только при наличии согласия
            //     //     if (canRequestAds)
            //     //     {
            //     //         ScreenProfiler.AddMessage("GDPR : Can request Ads");
            //     //         // ServiceLocator.Current.Get<ADSController>().Init()
            //     //         // оставил старый запуск сдк рекламы, иначе реклама не будет работать в игре
            //     //     }
            //     //     
            //     //     waitConsent = false;
            //     // });
            //     while (waitConsent) yield return null;
            //     // ***********************************************************************************************
            //
            //
            //     DLog.Alert("Start: load ADS", EDlogColor.YELLOW, SCREEN_LOG);
            //     SetProcessText("Load M-II");
            //     SetProgress(48);
            //     
            //     yield return StartCoroutine(ServiceLocator.Current.Get<AdController>().Init());
            // }
            // else
            // {
            //     ServiceLocator.Current.Get<AdController>().Inactive();
            // }
            
            
            
            
            
            SetProgress(50);
            GameObject _core = GameObject.Find("CORE");
            /*switch (GAME_MODE)
            {
                // старт с загрузкой сцены Lobby (для рабочей версии)
                case EGameMode.START_APP:
                    ScreenProfiler.AddMessage("Load Lobby");
                    //if (FIRST_LAUNCH)
                        //SceneManagement.I.LoadTutorial(false);
                    //else
                    ServiceLocator.Current.Get<SceneController>().LoadLobby(false, false);
                    break;
                // активация сцены с обучением
                case EGameMode.ACTIVATE_tutorial:
                    if(_core)
                    {
                        _core.GetComponent<IGameCore>().Activate();
                    }
                    else
                        Debug.LogError("В сцене отсутствует нужный файл-ядро => объект 'CORE'");
                    break;
                
                // активация уже существеющей сцены lobby
                case EGameMode.ACTIVATE_lobby:
                    ScreenProfiler.AddMessage("Activate Lobby");
                    if(_core)
                    {
                        _core.GetComponent<IGameCore>().Activate();
                    }
                    else
                        Debug.LogError("В сцене отсутствует нужный файл-ядро => объект 'CORE'");
                    break;
                
                // старт с созданием сцен Gameplay и Level
                case EGameMode.LOAD_gameplay:
                    
                    //SceneManagement.I.onContinueLoadData = LoadGameplay;
                    ServiceLocator.Current.Get<SceneController>().LoadLevel(1, false);
                    break;
                
                // сцены Gameplay и Level уже есть, просто делается активация
                case EGameMode.ACTIVATE_gameplay:
                    if(_core)
                    {
                        //SceneManagement.I.onContinueLoadData = LoadGameplay;
                        _core.GetComponent<IGameCore>().Activate();
                    }
                    else
                        Debug.LogError("В сцене отсутствует нужный файл-ядро => объект 'CORE'");
                    break;
            }*/
        }

        // * для новой игры
        void NewData()
        {
            // если первое включение приложения
            if (!USE_SAVING || !PlayerPrefs.HasKey(AppConstants.LAUNCH))
            {
                //ScreenProfiler.AddMessage("FIRST LAUNCH APP");
                PlayerPrefs.SetString(AppConstants.LAUNCH, "y");
                //FIRST_LAUNCH = true;
                // --------------- активация для новой игры
                //SceneManagement.I.onContinueLoadData += RefController.I.dataBase.gameStat.NewGame;
                //RefController.I.dataBase.gameStat.FirstStart();

               

                // -------------- в конце делаем сохранение при первом запуске
                //SceneManagement.I.onContinueLoadData = SaveManagement.I.SaveMobile;
            }
            // через загрузку
            // else
            // {
            //     //ScreenProfiler.AddMessage("SECOND LAUNCH APP");
            //     FIRST_LAUNCH = false;
            //     // SceneManagement.I.onContinueLoadData += добавляем все что нужно загружать
            //     //SceneManagement.I.onContinueLoadData = LoadGameplay;
            //     SaveManagement.I.LoadMobile();
            //     if(USE_SAVING)
            //         RefController.I.dataBase.gameStat.UpdateSaveData();
            // }
        }

        // * для загрузки
        void LoadData()
        {
            if (PlayerPrefs.HasKey(AppConstants.LAUNCH))
            {
                //ScreenProfiler.AddMessage("SECOND LAUNCH APP");
                //FIRST_LAUNCH = false;
                // SceneManagement.I.onContinueLoadData += добавляем все что нужно загружать
                //SceneManagement.I.onContinueLoadData = LoadGameplay;
                //SaveManagement.I.LoadMobile();
                //if(USE_SAVING)
                    //RefController.I.dataBase.gameStat.UpdateSaveData();
            }
        }



        void CompleteLobby()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
        }
        
        // для загрузки
        void LoadGameplay()
        {
            //DevManager.I.NewGame();
        }

        // полная готовность сцены (после снятия экрана загрузки)
         void CompleteGameplay()
        {
            //GameManager.StartBattle(BATTLE_MODE.survival_night);
            ServiceLocator.Current.Get<MonoBehaviourMaster>().freeze = false;
        }







         #region Status APP
         
         // всегда сохраняем текущее время при выходе из игры или потере фокуса


         // private void OnApplicationFocus(bool hasFocus)
         // {
         //     //if (!APP_LOAD || !GameSetup.I) return;
         //     //DLog.Alert("FOCUS");
         //
         //     if (hasFocus)
         //     {
         //         // сброс блокировки, чтобы не блокировать игру
         //         CORT.LoadPay(false);
         //
         //         if (USE_SERVER)
         //         {
         //             new SERVER_Connect();
         //         }
         //     }
         //     // игрок свернул игру
         //     else
         //     {
         //         TimeManagement.SaveCurrTime();
         //         GAMEPLAY_old.Saving();
         //     }
         // }
         //
         // private void OnApplicationQuit()
         // {
         //     TimeManagement.SaveCurrTime();
         //     GAMEPLAY_old.Saving();
         // }

         #endregion

    }


    

}
