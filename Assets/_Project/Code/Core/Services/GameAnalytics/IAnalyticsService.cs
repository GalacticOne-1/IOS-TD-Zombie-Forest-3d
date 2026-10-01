using System;

namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Единственный контракт аналитики для остального кода.
    /// Никогда не бросает исключений и никогда не блокирует игру.
    /// </summary>
    public interface IAnalyticsService
    {
        bool IsReady { get; }
        bool IsEnabled { get; }
 
        void LogEvent(AnalyticsEvent analyticsEvent);
        void SetUserProperty(string name, string value);
 
        /// <summary>Включить/выключить сбор (гейт для consent / настроек пользователя).</summary>
        void SetEnabled(bool enabled);
 
        /// <summary>Пока scope не disposed, события отбрасываются
        /// (debug-операции туториала и т.п.).</summary>
        IDisposable Suppress();
    }
}