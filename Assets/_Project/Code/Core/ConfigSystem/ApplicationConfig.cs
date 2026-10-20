using UnityEditor;
using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "ApplicationConfigs", menuName = "Game Configs/Core/Application Configs")]
    public class ApplicationConfig : ScriptableObject
    {
        
        public enum EStartApp
        {
            RELEASE,                     // обычная игра (загрузка лобби)
            Map,
            Home,
            Location,
            DevScene
        }
        
        public bool requiresLogo;
        
        public EStartApp startAppScene;
        public bool modeRegular;
        public bool showScreenLogs;
        public bool showCrashButton;
        
        [Space]
        public bool requiresSavingService;
        public bool requiresServerConnection;
        public bool requiresIntroService;
        public bool requiresIapService;
        public bool requiresAnalyticsService;
        public bool requiresAdService;
        
        [Space]
        public bool isAppstore;
        public bool adTestMode;
        
        
        [Space]
        public EPlayerController playerControllerType;
        public enum EPlayerController { NON, MOBILE, KEYBOARD }


        [HideInInspector]
        public int startingLocationId;
        
        
        
        
        
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        
    }
}