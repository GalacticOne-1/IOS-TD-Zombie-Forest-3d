using Firebase.Crashlytics;
using Galactic1.Code.Systems.Analytics.Providers;

namespace Galactic1.Mobile
{
    /// <summary>
    /// Теперь только Crashlytics. Аналитика живёт в GameAnalyticsService.
    /// FirebaseApp инициализируется общим FirebaseBootstrapper (один раз, не блокирует игру).
    /// </summary>
    public class FBA
    {
        /// <summary>Вызывать для краша приложения</summary>
        public static void CRASH() => throw new System.Exception("CRASH");

        /// <summary>Отправить сообщение в crashlytics</summary>
        public static void CRASH(string value) => Crashlytics.Log(value);

        /// <summary>Отправить ключ/значение в crashlytics</summary>
        public static void CRASH(string key, string value) => Crashlytics.SetCustomKey(key, value);

        /// <summary>Активация Crashlytics. Не блокирует; не шлёт аналитических событий.</summary>
        public static void Init()
        {
            FirebaseBootstrapper.Ensure(ok =>
            {
                if (!ok) return;

                // Crashlytics репортит все необработанные исключения как fatal (рекомендованное поведение).
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                DLog.Alert("Crashlytics launch");
            });
        }
    }
}