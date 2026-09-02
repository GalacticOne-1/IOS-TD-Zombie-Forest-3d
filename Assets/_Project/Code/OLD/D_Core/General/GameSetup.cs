
using System;
using System.Collections;
using DEV;
using Galactic1.Mobile;
using Galactic1.Test;
using UnityEngine;
using Galactic1.Core;

namespace Galactic1
{
    // главный загрузчик game scene
    public class GameSetup : Singleton<GameSetup>
    {
        //[FormerlySerializedAs("serviceLocatorStarter")] [SerializeField] private CampServiceLocatorAssembler campServiceLocatorStarter;
        [SerializeField] private bool requireUiCamera;
        [SerializeField] private Canvas[] canvas;

        /// для установления канваса
        public void SetCanvas(bool ui_mode)
        {
            for (int i = 0; i < canvas.Length; i++)
            {
                canvas[i].renderMode = ui_mode ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
            }
        }
        
        
        /// <summary>
        /// событие после снятия loadscreen
        /// </summary>
        public Action onComplete;
        
        /// <summary>
        /// при смене сцен, отписываемся от MonoBehaviourMaster
        /// </summary>
        public event DFunc onResetUpdate;

        

        public byte process { set; get; }

        

        
        
        
        // делает все нужные загрузки и подготавливает уровень 
        // перед снятием loadscreen
        public void Activate()
        {
            StartCoroutine(activate());
        }
       
        
        IEnumerator activate()
        {
            // важная часть !!! запрос к серверу
            // если прила застряла на лого, то проблема скорее всего здесь.
            ApplicationSetup.I.SetProcessText("Response of server");
            ApplicationSetup.I.SetProgress(55);
            
            // обновление всех канвасов после загрузки сцены, что бы расчеты с виджетами шли без ошибок
            Canvas.ForceUpdateCanvases();

            new TUTORIAL_Check_Status();
            
            // ***          ASSEMBLER           ***
            ScreenProfiler.AddMessage("ASSEMBLER".SetText(EDlogColor.YELLOW));
            //campServiceLocatorStarter.Initialize();
            //ServiceLocator.Current.Get<IconHub>().Initialize();
            ServiceLocator.Current.Get<ViewGameController>().Init();
            //ServiceLocator.Current.Get<ViewLevelController>().Init();
            //ServiceLocator.Current.Get<StatController>().Init();
            
            ApplicationSetup.I.SetProgress(56);
            
            
            //  *
            bool wait = true;
            float delay = 0;
            Coroutine corSerever;
            if (ApplicationSetup.I.USE_SERVER)
            {
                // count try for connect to server
                for (int i = 0; i < 10; i++)
                {
                    ScreenProfiler.AddMessage($"TRY CONNECT {i}");
                    DLog.Alert($"TRY CONNECT {i}", EDlogColor.YELLOW);
                    
                    // #1 делаем запрос
                    corSerever = StartCoroutine(TimeManagement.CheckDailyTime(() =>
                        {
                            wait = false;
                        },
                        () =>
                        {
                            delay = 3;
                            ApplicationSetup.I.SetProcessText("Server not response");
                            DLog.Alert($"NOT CONNECT",EDlogColor.RED);
                            wait = false;
                        }));

                    // #2 ждем
                    for (float w = ApplicationSetup.I.tryConnectWait; w >= 0; w -= Time.deltaTime)
                    {
                        // #3 проверяем подключение
                        if (TimeManagement.COMPLETE)
                        {
                            ScreenProfiler.AddMessage($"CONNECT COMPLETE");
                            DLog.Alert($"CONNECT COMPLETE");
                            delay = 0;
                            wait = false;
                            break;
                        }
                        
                        yield return null;
                    }

                    if (TimeManagement.COMPLETE) break;
                    
                    StopCoroutine(corSerever);
                    if (i == 2)
                    {
                        ScreenProfiler.AddMessage($"CONNECT FALSE");
                        wait = false;
                        new SERVER_ConnectError();
                        yield break;
                    }
                }

                if (delay > 0)
                {
                    ScreenProfiler.AddMessage($"CONNECT FALSE");
                    new SERVER_ConnectError();
                    yield break;
                }
            }
            else
            {
                wait = false;
            }
            
            ApplicationSetup.I.SetProcessText($"Almost )) {wait}");

#if !UNITY_EDITOR
            yield return new WaitForSeconds(.3f);
#endif
            while (wait) yield return null;
            
            ApplicationSetup.I.SetProcessText($"Connect log {delay}");
            if (ApplicationSetup.I.connectLog)
                yield return new WaitForSeconds(delay);
            // -------- сервер
            
            
            // базовая настройка уровня
            ApplicationSetup.I.SetProcessText("Finish");
            ApplicationSetup.I.SetProgress(56);
            Starter();
            //ServiceLocator.Current.Get<LocalisationController>().Activator();
            ApplicationSetup.I.SetProgress(57);
            ServiceLocator.Current.Get<LobbyButtons>().Activator();
            ApplicationSetup.I.SetProgress(58);
            
            //RawUI.camera = GameObject.Find("RAW_camera");
            ApplicationSetup.I.SetProcessText("A");
            yield return new WaitForSeconds(.2f);
            
            // ----
            ApplicationSetup.I.SetProcessText("B");
            
            // инициализация разных ассетов 
            //LibController.I.AssetInit();

            
            // если игра идет в одной сцене
            ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new []{ECanvas.GAME, ECanvas.LEVEL, ECanvas.OVER});
            // Если канвасам требуется камера из сцены CORE
            if (requireUiCamera)
            {
                var cam = GameObject.Find("UI Camera").GetComponent<Camera>();
                for (int i = 0; i < canvas.Length; i++)
                    canvas[i].worldCamera = cam;
            }
            //
            
            // * отображаем нужные элементы статистики
            ServiceLocator.Current.Get<ViewGameController>().GetStats(
                new[] { EBankResourceType.PlayerRank, EBankResourceType.CurrencySoft, EBankResourceType.CurrencyPremium, });
            
            ApplicationSetup.I.SetProgress(70);
            ScreenProfiler.AddMessage("Prepare");
            
            // загрузка данных ... (only load)
            //RefController.I.dataBase.gameStat.PrepareForLaunchApp();
            //ApplicationSetup.I.SetProgress(72);
            // ---------
            // ---------
            // ---------
            // --------- ^^ после загрузки данных
            if (ApplicationSetup.I.USE_IAP)
            {
                //IAPWidget.I.Activator();
            }

            // *** инициализация кнопок рекламы
            //new AD_Buttons().Subscription();
            //new AD_Buttons().Update();
            
            
            // инициализация компонентов прогресса
            //GameSpeedCntr.Activate();
            
            //Achievements.I.Activator();
            //LotteryWidget.I.Activator();
            
            
            ApplicationSetup.I.SetProgress(76);
            
            //WSkill.I.Activator();
            //PoolFloat.I.Activator();
            //Pool.I.Activator();
            
            
            // -----------------------
            
            // *** создаем сетку перед Bootstrap 
            //GridController.I.CreateGrid();
            
            ApplicationSetup.I.SetProgress(80);
            // состояние игры
            //ServiceLocator.Current.Get<Bootstrap>().Starter(!GAMEPLAY_old.DataGameplay().isGame ? EStarterState.NEW_GAME : EStarterState.LOADING);
            
            ApplicationSetup.I.SetProgress(81);
            //  ***      CREATE WIDGETS         ***
            //ServiceLocator.Current.Get<ViewGameController>().LocationViewModel.LoadContent();
            //ServiceLocator.Current.Get<ViewGameController>().UnitMngmViewModel.LoadContent();
            //ServiceLocator.Current.Get<ViewGameController>().EnrichmentViewModel.LoadContent();
            //ServiceLocator.Current.Get<PlayerNamePresenter>().Activator();
            //ServiceLocator.Current.Get<ViewGameController>().MapViewModel.Activator();
            
            // *** стартовая загрузка виджетов для основного экрана 
            //EventBus<ScreenLoadRegularEvent>.Raise(new ScreenLoadRegularEvent());
            
            ApplicationSetup.I.SetProgress(82);
            
            ScreenProfiler.AddMessage("97 a");
            //DEV_polygon.I.LoadPolygon();
            InteractionPolygon.I.LoadPolygon();
            ToolPolygon.I.LoadPolygon();
            
            ApplicationSetup.I.SetProgress(85);
            new TUTORIAL_Start();
            
            ApplicationSetup.I.SetProgress(86);
            //ServiceLocator.Current.Get<TaskController>().LoadTask();
            
            
            
            
            // ***          ПРОВЕРКА ЗАВИСИМОСТЕЙ ОТ ПРОГРЕССА В ИГРЕ И БАЗОВЫЙ ЗАПУСК           ***
            ApplicationSetup.I.SetProgress(90);
            ScreenProfiler.AddMessage("Dependencies check");

            //var level = DeveloperConsole.I.game.open_menu ? 1000 : GAMEPLAY_old.PlayerRank;
            //ServiceLocator.Current.Get<AccessController>().LoadAccessMainMenu(level);
            //ServiceLocator.Current.Get<AccessController>().LoadAccessWorld(level);
            
            
            
            // *** если игрок находится на карте, сразу загружаем карту
            // if (ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value.movement_to_location || 
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value.cur_location != 0)
            // {
            //     ServiceLocator.Current.Get<GameMachine>().Map_Enter();
            //     yield return new WaitForSeconds(2);
            // }
            
            // *** стартовая загрузка виджетов для основного экрана 
            // else
            // {
            //     EventBus<ScreenLoadRegularEvent>.Raise(new ScreenLoadRegularEvent());
            // }
            
            
            
            ServiceLocator.Current.Get<GameMachine>().GameInit();
            while (process > 0) yield return null;
            
            yield return new WaitForSeconds(.3f);
            
            //RefController.I.dataBase.gameStat.PrepareForLaunchApp();
            
            //Tutorial.I.Activator();
            // что бы канвас (world) работал
            /*GameObject.Find("Canvas_game").GetComponent<Canvas>().worldCamera =
               GameObject.Find("Main Camera").GetComponent<Camera>();*/
            
            ApplicationSetup.I.SetProcessText("C");
            // Если есть подключение к серверу, проверяем оффлайн зароботок, экспедиции и пр
            if (TimeManagement.COMPLETE)
            {
                //IAPOffer.I.Load();
                //IAPConvert.I.LoadStarterPack();
                //OfflineReward.I.CheckOfflineReward();
            }
            //
            
            ApplicationSetup.I.SetProgress(98f);

            ApplicationSetup.I.SetProcessText("D");
            DLog.Alert("Start: saving",EDlogColor.YELLOW);
            //GAMEPLAY_old.Saving();
            //ServiceLocator.Current.Get<SceneController>().onContinueLoadData?.Invoke();
            yield return new WaitForSeconds(ApplicationSetup.I.durationLoad);
            
            // непосредственно при запуске игры (снятие заставки)
            ServiceLocator.Current.Get<MusicManagement>().Lobby();
            EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());

            // в самом конце, когда все готово
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadScreen, 
               // ApplicationSetup.I.lobbyComplete);
            
            ApplicationSetup.I.logo.SetActive(false);
            //ApplicationSetup.APP_LOAD = true;
            //new ANALITICS_FOR_GAMEPLAY(ANALITICS_FOR_GAMEPLAY.ERequestGameplay.Game_Launch);
            
            // панель для блокировка экрана до готовности приложения
            //FindObjectOfType<AppLoading>(true).gameObject.SetActive(false);
            
            DLog.Alert($"***             Game Setup Complete! {Time.time}          ***");
        }

        public void Reset()
        {
            ServiceLocator.Current.Get<MusicManagement>().LobbyStop();
            onResetUpdate?.Invoke();
            onResetUpdate = null;
        }
        
        
        // если нужно присвоить ссылки для глобального дуступа
        public void Starter()
        {
           
        }

    }

}