using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
        public bool requiresReviewService;
        public bool requiresIntroService;
        public bool requiresIapService;
        public bool requiresAnalyticsService;
        public bool requiresAdService;
        public bool requiresTutorial;
        
        [Space]
        public bool isAppstore;
        public bool adTestMode;
        
        
        [Space]
        public EPlayerController playerControllerType;
        public enum EPlayerController { NON, MOBILE, KEYBOARD }


        [HideInInspector]
        public int startingLocationId;
        
        
        
        
        
        
        //public bool FIRST_LAUNCH;
        //public bool APP_LOAD = false;                  // приложение загружено
        
        
        
        #region Progress load app

        private float currentProgress;
        
        // полоса готовности игры
        public void SetProgress(float newCurr)
        {
            //tProgress.text = $"{newCurr}%";
            //progressBar.fillAmount = newCurr / 100.0f;
        }
        
        /// <summary>
        /// Для указания названия процесса 
        /// </summary>
        /// <param name="t"></param>
        public void SetProcessText(string t)
        {
            //processTitle.text = t;
        }


        
        #endregion







        #region GAME SETTINGS PARAMS


        private float musicVolume;
        private float sfxVolume;
        
        

        #endregion
        
        
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        
    }
}