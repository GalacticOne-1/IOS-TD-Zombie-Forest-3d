using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;

namespace Galactic1.Mobile
{
    public class FBA
    {

        /*
         *     Аналитика Firebase
         */

        
        
        /// <summary>
        /// Вызывать для краша приложения
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void CRASH() => throw new System.Exception("CRASH");
        
        /// <summary>
        /// Отрпавить сообщеение в crashlitycs
        /// </summary>
        /// <param name="message"></param>
        public static void CRASH(string value) => Crashlytics.Log(value);
        /// <summary>
        /// Отрпавить сообщеение в crashlitycs
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void CRASH(string key, string value) => Crashlytics.SetCustomKey(key, value);
        
        
        /// <summary>
        /// Активация аналитики
        /// </summary>
        public static void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    // Crashlytics will use the DefaultInstance, as well;
                    // this ensures that Crashlytics is initialized.
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    

                    // When this property is set to true, Crashlytics will report all
                    // uncaught exceptions as fatal events. This is the recommended behavior.
                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                    // Set a flag here for indicating that your project is ready to use Firebase.
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
                    DLog.Alert("Analitics launch");
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}",dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
                
                //SDKStatus.I.StatusProcess(SDKStatus.EAppSetup.analittics, true);
            });
        }


        
        
    }
}