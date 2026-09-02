using Galactic1.Configs;
using Galactic1;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class ApplicationConfigsEditor : ExtendedEditorWindow
    {
        
        
        private ApplicationConfig script;
        private int tab;


        private ConfigProvider _configsProvider;


        private string[] tTab = new[]
        {
            "Logo", "Game Mode", "Setup", "Check"
        };


        private SerializedProperty processTitle, 
            tProgress,
            progressBar,
            logo;
        
        
        
        
        
        
        
        [MenuItem("Add/Application Setup", false, 0)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<ApplicationConfigsEditor>();
            window.titleContent = new GUIContent("Настройка приложения");
            window.Show();
            window.minSize = new Vector2(700f, 450);
            
            
        }
        
        
        private void OnEnable()
        {
            _configsProvider = new ConfigProvider();

            script = _configsProvider.Get<ApplicationConfig>();

            // prop
            //processTitle = scriptSO.FindProperty("processTitle");
            //tProgress = scriptSO.FindProperty("tProgress");
            //progressBar = scriptSO.FindProperty("progressBar");
            //logo = scriptSO.FindProperty("logo");
        }

        void OnDisable()
        {
            if (script)
            {
                script.Save();
            }

            _configsProvider.Get<GameConfig>().Save();
            _configsProvider = null;
        }




        public void OnGUI()
        {
            // if (script == null)
            // {
            //     EditorGUILayout.Space(50);
            //     EditorGUILayout.HelpBox("Включите сцену CORE",
            //         MessageType.Warning);
            //     return;
            // }
            EditorGUI.BeginChangeCheck();
            
            ScriptableObject scriptableObj = this;
            serialObj = new SerializedObject(scriptableObj);
            
            serialObj.Update();
            
            

            // top menu
            EditorGUILayout.Space(10);
            var t  = GUILayout.Toolbar(tab, tTab, GUILayout.MaxWidth(320), GUILayout.MaxHeight(30));
            if (tab != t)
            {
                GUI.FocusControl(null);
            }
            tab = t;
            EditorGUILayout.Space(20);
            //
            
           
            // main block
            EditorGUILayout.BeginVertical("box");
            switch (tab)
            {
                case 0:
                    Logo();
                    break;
                case 1:
                    GameMode();
                    break;
                case 2:
                    Setup();
                    break;
                case 3:
                    Check();
                    break;
            }
            EditorGUILayout.EndVertical();
            // ---- main block


                
            if (EditorGUI.EndChangeCheck()) GUI.FocusControl(null);
            EditorEndCheck();
            
        }




        void Logo()
        {
            //EditorGUILayout.PropertyField(processTitle, false);
            //EditorGUILayout.PropertyField(tProgress, false);
            //EditorGUILayout.PropertyField(progressBar, false);
            //EditorGUILayout.PropertyField(logo, false);
            //script.durationLoad = EditorGUILayout.FloatField("Duration Load",script.durationLoad);
        }

        void GameMode()
        {

            var sapp = "";

            switch (script.startAppScene)
            {
                case ApplicationConfig.EStartApp.RELEASE:
                    sapp = "РЕЛИЗ";
                    break;
                case ApplicationConfig.EStartApp.Map:
                    sapp = "ЗАПУСК СО СЦЕНЫ КАРТЫ";
                    break;
                case ApplicationConfig.EStartApp.Home:
                    sapp = "ЗАПУСК СО СЦЕНЫ ЛАГЕРЯ";
                    break;
                case ApplicationConfig.EStartApp.Location:   
                    sapp = "ЗАГРУЗКА СО СЦЕНЫ ЛОКАЦИИ";
                    break;
                case ApplicationConfig.EStartApp.DevScene:   
                    sapp = "ЗАГРУЗКА ТЕСТОВОЙ СЦЕНЫ ";
                    break;
            }
            
            EditorGUILayout.HelpBox($"Старт приложения: {sapp}", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            //script.GAME_MODE = (EGameMode)EditorGUILayout.EnumPopup("GAME MODE", script.GAME_MODE);
            
            Button(new CButtonData()
            {
                name = "RELEASE",
                func = () =>
                {
                    if (script.startAppScene != ApplicationConfig.EStartApp.RELEASE)
                        script.startAppScene = ApplicationConfig.EStartApp.RELEASE;
                },
                enabled = script.startAppScene == ApplicationConfig.EStartApp.RELEASE,
            });
            Button(new CButtonData()
            {
                name = "MAP",
                func = () =>
                {
                    if (script.startAppScene != ApplicationConfig.EStartApp.Map)
                        script.startAppScene = ApplicationConfig.EStartApp.Map;
                },
                enabled = script.startAppScene == ApplicationConfig.EStartApp.Map,
            });
            Button(new CButtonData()
            {
                name = "HOME",
                func = () =>
                {
                    if (script.startAppScene != ApplicationConfig.EStartApp.Home)
                        script.startAppScene = ApplicationConfig.EStartApp.Home;
                },
                enabled = script.startAppScene == ApplicationConfig.EStartApp.Home,
                width = 120
            });
            Button(new CButtonData()
            {
                name = "LOCATION",
                func = () =>
                {
                    if (script.startAppScene != ApplicationConfig.EStartApp.Location)
                        script.startAppScene = ApplicationConfig.EStartApp.Location;
                },
                enabled = script.startAppScene == ApplicationConfig.EStartApp.Location,
            });
            
            Button(new CButtonData()
            {
                name = "DEV SCENE",
                func = () =>
                {
                    if (script.startAppScene != ApplicationConfig.EStartApp.DevScene)
                        script.startAppScene = ApplicationConfig.EStartApp.DevScene;
                },
                enabled = script.startAppScene == ApplicationConfig.EStartApp.DevScene,
            });
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            
            // 
            if (script.startAppScene == ApplicationConfig.EStartApp.Location)
            {
                EditorGUILayout.HelpBox(
                    "Выбрать ID для создания нужной локации (0 нельзя, т.к это лагерь)",
                    MessageType.Info);
                script.startingLocationId = EditorGUILayout.IntField(
                    "Location Id",
                    script.startingLocationId,
                    GUILayout.Width(180));

                if (script.startingLocationId == 0) script.startingLocationId = 1;
            }
            
            
            EditorGUILayout.Space(20);

            EditorGUILayout.HelpBox(AppConstants.SERVER_ON
                    ? "Игра требует подключения к серверу"
                    : "Игре не требуется интернет",
                MessageType.Warning);
            Button(new CButtonData()
            {
                name = "SERVER",
                //func =  () => script.requiresServerConnection = !script.requiresServerConnection,
                enabled = AppConstants.SERVER_ON,
            });
            
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Отображение логов на экране",
                MessageType.Warning);
            Button(new CButtonData()
            {
                name = "SCREEN LOG",
                func =  () => script.showScreenLogs = !script.showScreenLogs,
                enabled = script.showScreenLogs,
            });
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Отображение кнопки краша",
                MessageType.Warning);
            Button(new CButtonData()
            {
                name = "BTN CRASH",
                func =  () => script.showCrashButton = !script.showCrashButton,
                enabled = script.showCrashButton,
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void Setup()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Для игрового магазина",
                MessageType.Info);
            Button(new CButtonData()
            {
                name = script.isAppstore ? "APPSTORE" : "GOOGLE PLAY",
                func =  () => script.isAppstore = !script.isAppstore,
                enabled = script.isAppstore,
                colorON = Color.yellow,
                colorOFF = Color.yellow
            });
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Наличие стора в игре",
                MessageType.Info);
            Button(new CButtonData()
            {
                name = script.requiresIapService ? "USE IAP" : "NOT IAP",
                func =  () => script.requiresIapService = !script.requiresIapService,
                enabled = script.requiresIapService,
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Что должно работать в приложении",
                MessageType.Info);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            Button(new CButtonData()
            {
                name = "SAVING",
                func =  () => script.requiresSavingService = !script.requiresSavingService,
                enabled = script.requiresSavingService,
            });
            Button(new CButtonData()
            {
                name = "ANALYTICS",
                func =  () => script.requiresAnalyticsService = !script.requiresAnalyticsService,
                enabled = script.requiresAnalyticsService,
            });
            Button(new CButtonData()
            {
                name = "ADS",
                func =  () => script.requiresAdService = !script.requiresAdService,
                enabled = script.requiresAdService,
            });
            Button(new CButtonData()
            {
                name = "REVIEW",
                func =  () => _configsProvider.Get<GameConfig>().SetReview = !_configsProvider.Get<GameConfig>().General.review,
                enabled = _configsProvider.Get<GameConfig>().General.review,
            });
            EditorGUILayout.EndHorizontal();
            
            
            // ---------
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            Button(new CButtonData()
            {
                name = "LOGO",
                func =  () => script.requiresLogo = !script.requiresLogo,
                enabled = script.requiresLogo,
            });
            //script.logo.SetActive(script.USE_LOGO);
            Button(new CButtonData()
            {
                name = "TUTORIAL",
                func =  () => _configsProvider.Get<GameConfig>().SetTutorial = !_configsProvider.Get<GameConfig>().General.tutorial,
                enabled = _configsProvider.Get<GameConfig>().General.tutorial,
            });
            Button(new CButtonData()
            {
                name = "INTRO",
                func =  () => script.requiresIntroService = !script.requiresIntroService,
                enabled = script.requiresIntroService,
            });
            
            EditorGUILayout.EndHorizontal();
            
            
            // -------
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            // 1
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Dev mode с читами",
                MessageType.Warning);
            Button(new CButtonData()
            {
                name = script.modeRegular ? "GAME MODE" : "DEV MODE",
                func = () =>
                {
                    script.modeRegular = !script.modeRegular;
                    SystemRepository.IsRelease = script.modeRegular;
                },
                enabled = script.modeRegular,
            });
            EditorGUILayout.EndVertical();
            
            // 2
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Для релиза должно быть зеленым",
                MessageType.Warning);
            Button(new CButtonData()
            {
                name = script.adTestMode ? "AD TEST MODE" : "AD WORK",
                func =  () => script.adTestMode = !script.adTestMode,
                enabled = script.adTestMode,
                colorON = Color.red,
                colorOFF = Color.green
            });
            EditorGUILayout.EndVertical();
            
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Тип управления объектом игрока",
                MessageType.Info);
            script.playerControllerType = 
                (ApplicationConfig.EPlayerController)EditorGUILayout.EnumPopup(script.playerControllerType);
            EditorGUILayout.EndVertical();
            
            //
            EditorGUILayout.EndHorizontal();
            
        }


        void Check()
        {
            
            // #1
            EditorGUILayout.HelpBox($"File save Data ===> {3}", MessageType.Warning);
            EditorGUILayout.Space(10);
            
            
            // #2
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Должно быть обязательно зеленым для релиза", MessageType.Warning);
            EditorGUILayout.Space(10);

            
            EditorGUILayout.BeginHorizontal();
            
            // -- 1 box
            EditorGUILayout.BeginVertical("box",GUILayout.MaxWidth(100));
            Label(new CLabelData() { name = "Saving", style = FlagStatusStyle(), enabled = script.requiresSavingService});
            Label(new CLabelData() { name = "Analitics", style = FlagStatusStyle(), enabled = script.requiresAnalyticsService});
            Label(new CLabelData() { name = "Ads", style = FlagStatusStyle(), enabled = script.requiresAdService});
            Label(new CLabelData() { name = "Ads Work", style = FlagStatusStyle(), enabled = !script.adTestMode});
            EditorGUILayout.EndVertical();
            
            // -- 2 box
            EditorGUILayout.BeginVertical("box",GUILayout.MaxWidth(100));
            Label(new CLabelData() { name = "Server", style = FlagStatusStyle(), enabled = script.requiresServerConnection});
            Label(new CLabelData() { name = "Screen Log", style = FlagStatusStyle(), enabled = !script.showScreenLogs});
            EditorGUILayout.EndVertical();
            
            // -- 3 box
            EditorGUILayout.BeginVertical("box",GUILayout.MaxWidth(100));
            Label(new CLabelData() { name = "Logo", style = FlagStatusStyle(), enabled = script.requiresLogo});
            Label(new CLabelData() { name = "Review", style = FlagStatusStyle(), enabled = _configsProvider.Get<GameConfig>().General.review});
            Label(new CLabelData() { name = "Game Mode", style = FlagStatusStyle(), enabled = script.modeRegular});
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            
            // #3
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Необязательно", MessageType.Warning);
            EditorGUILayout.Space(10);
            
            
            EditorGUILayout.BeginHorizontal();
            
            // -- 1 box
            EditorGUILayout.BeginVertical("box",GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField($"Controller {script.playerControllerType}", FlagStatusStyle());
            EditorGUILayout.Space(10);
            
            Label(new CLabelData() { name = "IAP", style = FlagStatusStyle(), enabled = script.requiresIapService});
            Label(new CLabelData() { name = "Tutorial", style = FlagStatusStyle(), enabled = _configsProvider.Get<GameConfig>().General.tutorial});
            EditorGUILayout.EndVertical();
            
            
            EditorGUILayout.EndHorizontal();
        }
        
    }
}