using System;
using Firebase.Analytics;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics.Providers
{
    /// <summary>
    /// ЕДИНСТВЕННЫЙ класс аналитики, который вызывает Firebase.Analytics.*.
    /// echoToConsole = true для Development build (дублирует события в консоль).
    /// </summary>
    public sealed class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        private readonly bool _echoToConsole;
        private bool _ready;

        public FirebaseAnalyticsProvider(bool echoToConsole = false)
        {
            _echoToConsole = echoToConsole;
        }

        public void Initialize(Action<bool> onComplete)
        {
            FirebaseBootstrapper.Ensure(ok =>
            {
                _ready = ok;
                onComplete?.Invoke(ok);
            });
        }

        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            if (!_ready) return;

            if (_echoToConsole)
                Debug.Log(DebugLogProvider.Format(analyticsEvent));

            int count = analyticsEvent.Params.Count;
            if (count == 0)
            {
                FirebaseAnalytics.LogEvent(analyticsEvent.Name);
                return;
            }

            var parameters = new Parameter[count];
            for (int i = 0; i < count; i++)
            {
                var p = analyticsEvent.Params[i];
                switch (p.Type)
                {
                    case AnalyticsParamType.String:
                        parameters[i] = new Parameter(p.Key, p.StringValue);
                        break;
                    case AnalyticsParamType.Long:
                        parameters[i] = new Parameter(p.Key, p.LongValue);
                        break;
                    default:
                        parameters[i] = new Parameter(p.Key, p.DoubleValue);
                        break;
                }
            }

            FirebaseAnalytics.LogEvent(analyticsEvent.Name, parameters);
        }

        public void SetUserProperty(string name, string value)
        {
            if (!_ready) return;
            FirebaseAnalytics.SetUserProperty(name, value);
        }

        public void SetCollectionEnabled(bool enabled)
        {
            if (!_ready) return;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(enabled);
        }
    }
}