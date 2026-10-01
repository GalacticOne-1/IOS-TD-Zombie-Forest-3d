using System.Collections;
using Galactic1.Code.Core.State;
using Galactic1.Code.Systems.Analytics.Integration;
using Galactic1.Code.Systems.Analytics.Providers;
using Galactic1.Code.Systems.Tutorial.Analytics;
using Galactic1.Code.Systems.Tutorial.Authoring;
using R3;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Точка сборки аналитики. Вызывается ОДИН раз из GameEntryPoint.LoadAndStartCore
    /// после LoadGameState и ДО CoreRegistrations.Register (TutorialService создаётся там
    /// и берёт ITutorialAnalytics из root-контейнера).
    ///
    /// Всё регистрируется в root-контейнере (живёт всё приложение). Не блокирует загрузку:
    /// Firebase стартует в фоне, пока он не готов события копятся в буфере сервиса.
    /// </summary>
    public static class AnalyticsRuntime
    {
        /// <summary>Версия контракта событий (не версия билда и не версия туториала).</summary>
        public const int SchemaVersion = 1;

        private static readonly float[] RetryDelays = { 5f, 30f, 120f };

        /// <summary>Статический доступ ТОЛЬКО для legacy-прослойки (AnalyticsService) на время миграции.
        /// Новый код получает IAnalyticsService из контейнера / конструктора.</summary>
        public static IAnalyticsService Service { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Service = null; // Play->Stop->Play без domain reload

        public static void Register(
            DIContainer root,
            MonoBehaviour runner,
            bool analyticsEnabled,
            TutorialCampaignRegistry tutorialRegistry,
            ReactiveProperty<CGameStateTutorial> tutorialState)
        {
            var service = new GameAnalyticsService(
                CreateProvider(analyticsEnabled),
                SchemaVersion,
                Debug.isDebugBuild ? "dev" : "prod");

            service.Start();

            if (analyticsEnabled)
                runner.StartCoroutine(RetryRoutine(service));

            Service = service;
            root.RegisterInstance<IAnalyticsService>(service);

            // --- Tutorial
            var clock = new AppForegroundClock();
            var tutorialAdapter = new TutorialAnalyticsAdapter(service, clock, tutorialRegistry, tutorialState);
            root.RegisterInstance<ITutorialAnalytics>(tutorialAdapter);
        }

        private static IAnalyticsProvider CreateProvider(bool analyticsEnabled)
        {
            if (!analyticsEnabled)
                return new NullAnalyticsProvider();

#if UNITY_EDITOR
            return new DebugLogProvider();
#else
            return new FirebaseAnalyticsProvider(echoToConsole: Debug.isDebugBuild);
#endif
        }

        /// <summary>Повторные попытки инициализации при Failed: 5 / 30 / 120 секунд, затем стоп.</summary>
        private static IEnumerator RetryRoutine(GameAnalyticsService service)
        {
            foreach (var delay in RetryDelays)
            {
                yield return new WaitForSecondsRealtime(delay);

                if (service.IsReady)
                    yield break;

                service.Start(); // игнорируется, если ещё Initializing
            }
        }
    }
}
