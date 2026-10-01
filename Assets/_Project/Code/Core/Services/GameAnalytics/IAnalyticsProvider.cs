using System;

namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Бэкенд аналитики. Реализации: Firebase / DebugLog / Null.
    /// Все методы вызываются из main thread. Исключения перехватывает GameAnalyticsService.
    /// </summary>
    public interface IAnalyticsProvider
    {
        /// <summary>Асинхронная инициализация. onComplete(true) = готов, false = недоступен.
        /// Колбэк ОБЯЗАН прийти в main thread.</summary>
        void Initialize(Action<bool> onComplete);

        void LogEvent(AnalyticsEvent analyticsEvent);
        void SetUserProperty(string name, string value);
        void SetCollectionEnabled(bool enabled);
    }
}