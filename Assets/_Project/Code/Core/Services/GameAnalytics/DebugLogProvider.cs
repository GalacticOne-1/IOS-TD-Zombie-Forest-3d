using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics.Providers
{
    /// <summary>Пишет события в консоль Unity. Используется в Editor.</summary>
    public sealed class DebugLogProvider : IAnalyticsProvider
    {
        public static string Format(AnalyticsEvent e) => $"[Analytics] {e}";

        public void Initialize(Action<bool> onComplete) => onComplete?.Invoke(true);

        public void LogEvent(AnalyticsEvent analyticsEvent) => Debug.Log(Format(analyticsEvent));

        public void SetUserProperty(string name, string value) =>
            Debug.Log($"[Analytics] user_property {name}={value}");

        public void SetCollectionEnabled(bool enabled) =>
            Debug.Log($"[Analytics] collection enabled = {enabled}");
    }
}