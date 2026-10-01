using System;
using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Code.Systems.Tutorial.Analytics;
using Galactic1.Code.Systems.Tutorial.Authoring;
using R3;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics.Integration
{
    /// <summary>
    /// ЕДИНСТВЕННЫЙ путь tutorial -> аналитика: TutorialService -> ITutorialAnalytics -> этот adapter.
    /// TutorialService/TutorialTaskPresenter/ScenarioTaskService не знают про аналитику.
    ///
    /// Контракт:
    ///  - tutorial_run_id = "{guid8}-{startedAtUnixSeconds}" (из сохранения, переживает рестарт;
    ///    при fallback-Restart timestamp не меняется -> run тот же).
    ///  - Resume после рестарта выводится сам: первое упоминание кампании в процессе без
    ///    TutorialStarted/TutorialResumed => tutorial_resume(resume_kind=current).
    ///  - Шаг, завершённый без StepStarted (мгновенный) => step_complete с instant=1, duration=0.
    ///  - duration_sec = foreground-секунды между step_start и step_complete.
    ///  - tutorial_abandon нет: abandonment считается запросом по данным.
    ///  - Любое исключение внутри adapter перехватывается: туториал от аналитики не зависит.
    /// </summary>
    public sealed class TutorialAnalyticsAdapter : ITutorialAnalytics
    {
        private const int MaxStringLength = 100;

        private readonly IAnalyticsService _analytics;
        private readonly AppForegroundClock _clock;
        private readonly TutorialCampaignRegistry _registry;
        private readonly ReactiveProperty<CGameStateTutorial> _state;
        private readonly Dictionary<string, string> _stepKeyCache = new();

        private string _campaignGuid;
        private string _runId;
        private bool _runKnown;
        private bool _nextStepResumed;

        private string _activeStepGuid;
        private double _activeStepClock;
        private bool _activeStepResumed;

        public TutorialAnalyticsAdapter(
            IAnalyticsService analytics,
            AppForegroundClock clock,
            TutorialCampaignRegistry registry,
            ReactiveProperty<CGameStateTutorial> state)
        {
            _analytics = analytics;
            _clock = clock;
            _registry = registry;
            _state = state;
        }

        // =========================================================
        // ITutorialAnalytics
        // =========================================================

        public void TutorialStarted(string campaignId) => Safe(() =>
        {
            BeginRun(campaignId);
            _analytics.LogEvent(Base(AnalyticsEventNames.TutorialBegin));
        });

        /// <summary>Явный resume (checkpoint / continue). Resume-current сюда не приходит —
        /// его выводит EnsureRun.</summary>
        public void TutorialResumed(string campaignId) => Safe(() =>
        {
            BeginRun(campaignId);
            _nextStepResumed = true;
            _analytics.LogEvent(Base(AnalyticsEventNames.TutorialResume)
                .Add(AnalyticsParams.ResumeKind, "checkpoint"));
        });

        public void TutorialResumeFallback(string campaignId, string savedCurrentStepId, string checkpointStepId,
            string currentDomain, string reason) => Safe(() =>
        {
            // Прогресс сброшен, но startedAtUnixSeconds не меняется -> run_id прежний.
            BeginRun(campaignId);
            _analytics.LogEvent(Base(AnalyticsEventNames.TutorialResumeFallback)
                .Add(AnalyticsParams.Reason, Trunc(reason))
                .Add(AnalyticsParams.CurrentDomain, Trunc(currentDomain)));
        });

        public void StepStarted(string campaignId, string chapterId, string stepId, int stepIndex) => Safe(() =>
        {
            EnsureRun(campaignId);

            bool resumed = _nextStepResumed;
            _nextStepResumed = false;

            _activeStepGuid = stepId;
            _activeStepClock = _clock.Now;
            _activeStepResumed = resumed;

            _analytics.LogEvent(StepBase(AnalyticsEventNames.TutorialStepStart, chapterId, stepId, stepIndex)
                .Add(AnalyticsParams.Resumed, resumed));
        });

        public void StepCompleted(string campaignId, string chapterId, string stepId, int stepIndex) => Safe(() =>
        {
            EnsureRun(campaignId);

            bool wasShown = _activeStepGuid == stepId;
            double duration = wasShown ? Math.Max(0d, _clock.Now - _activeStepClock) : 0d;
            bool resumed = wasShown && _activeStepResumed;
            ClearActiveStep();

            _analytics.LogEvent(StepBase(AnalyticsEventNames.TutorialStepComplete, chapterId, stepId, stepIndex)
                .Add(AnalyticsParams.Instant, !wasShown)
                .Add(AnalyticsParams.Resumed, resumed)
                .Add(AnalyticsParams.DurationSec, Math.Round(duration, 1)));
        });

        /// <summary>Skip достижим только из debug API (подавляется guard'ом) — события в v1 нет.</summary>
        public void StepSkipped(string campaignId, string chapterId, string stepId, int stepIndex) =>
            Safe(ClearActiveStep);

        /// <summary>Чекпоинты нужны только для диагностики resume — в Firebase не отправляем.</summary>
        public void CheckpointReached(string campaignId, string stepId) { }

        public void TutorialCompleted(string campaignId) => Safe(() =>
        {
            EnsureRun(campaignId);
            ClearActiveStep();
            _analytics.LogEvent(Base(AnalyticsEventNames.TutorialComplete));
        });

        /// <summary>В production вызывается только при ошибке графа (debug-restart подавляется).
        /// Событие не шлём — abandonment выводится запросом.</summary>
        public void TutorialAbandoned(string campaignId, string lastStepId) => Safe(() =>
        {
            Debug.LogWarning($"[Analytics] TutorialAbandoned (campaign={campaignId}, lastStep={lastStepId})");
            ClearActiveStep();
        });

        // =========================================================
        // INTERNAL
        // =========================================================

        private void BeginRun(string campaignGuid)
        {
            _campaignGuid = campaignGuid;
            _runId = BuildRunId(campaignGuid);
            _runKnown = true;
            _nextStepResumed = false;
            ClearActiveStep();
        }

        /// <summary>Первое упоминание кампании в этом процессе без TutorialStarted/Resumed —
        /// значит, это resume после рестарта приложения (ветка ResumeCurrent, которую
        /// TutorialService сам не репортит).</summary>
        private void EnsureRun(string campaignGuid)
        {
            if (_runKnown && _campaignGuid == campaignGuid)
                return;

            BeginRun(campaignGuid);
            _nextStepResumed = true;
            _analytics.LogEvent(Base(AnalyticsEventNames.TutorialResume)
                .Add(AnalyticsParams.ResumeKind, "current"));
        }

        private string BuildRunId(string campaignGuid)
        {
            string prefix = string.IsNullOrEmpty(campaignGuid)
                ? "unknown"
                : (campaignGuid.Length > 8 ? campaignGuid.Substring(0, 8) : campaignGuid);

            long started = _state.Value.startedAtUnixSeconds;
            return started > 0 ? $"{prefix}-{started}" : $"{prefix}-legacy";
        }

        private AnalyticsEvent Base(string eventName) =>
            new AnalyticsEvent(eventName)
                .Add(AnalyticsParams.CampaignId, _campaignGuid)
                .Add(AnalyticsParams.TutorialRunId, _runId);

        private AnalyticsEvent StepBase(string eventName, string chapterId, string stepId, int stepIndex) =>
            Base(eventName)
                .Add(AnalyticsParams.ChapterId, chapterId)
                .Add(AnalyticsParams.StepId, stepId)
                .Add(AnalyticsParams.StepKey, ResolveStepKey(stepId))
                .Add(AnalyticsParams.StepIndex, stepIndex);

        /// <summary>DebugKey = имя ассета. Только для чтения человеком; identity — step_id (Guid).</summary>
        private string ResolveStepKey(string stepGuid)
        {
            if (string.IsNullOrEmpty(stepGuid))
                return null;

            if (_stepKeyCache.TryGetValue(stepGuid, out var cached))
                return cached;

            var step = _registry.GetCampaignByGuid(_campaignGuid)?.GetStepByGuid(stepGuid);
            string key = step != null && step.stepId != null ? Trunc(step.stepId.DebugKey) : null;
            _stepKeyCache[stepGuid] = key;
            return key;
        }

        private void ClearActiveStep()
        {
            _activeStepGuid = null;
            _activeStepResumed = false;
        }

        private static string Trunc(string value) =>
            string.IsNullOrEmpty(value) || value.Length <= MaxStringLength
                ? value
                : value.Substring(0, MaxStringLength);

        private static void Safe(Action action)
        {
            try { action(); }
            catch (Exception ex) { Debug.LogWarning($"[Analytics] TutorialAnalyticsAdapter error: {ex.Message}"); }
        }
    }
}
