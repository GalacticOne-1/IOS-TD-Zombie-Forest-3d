using System;

namespace Galactic1.Code.Systems.Analytics.Providers
{
    /// <summary>Полностью безопасный no-op (analytics выключена конфигом).</summary>
    public sealed class NullAnalyticsProvider : IAnalyticsProvider
    {
        public void Initialize(Action<bool> onComplete) => onComplete?.Invoke(true);
        public void LogEvent(AnalyticsEvent analyticsEvent) { }
        public void SetUserProperty(string name, string value) { }
        public void SetCollectionEnabled(bool enabled) { }
    }
}