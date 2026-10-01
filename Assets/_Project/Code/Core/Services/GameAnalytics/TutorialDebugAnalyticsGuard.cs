using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Analytics.Integration
{
    /// <summary>
    /// Декоратор над ITutorialDebugService: на время debug-операций события не отправляются,
    /// поэтому CompleteStepDebug/ForceStep/RestartTutorial не попадают в tutorial funnel.
    /// Состояние TutorialAnalyticsAdapter при этом обновляется как обычно.
    /// </summary>
    public sealed class TutorialDebugAnalyticsGuard : ITutorialDebugService
    {
        private readonly ITutorialDebugService _inner;
        private readonly IAnalyticsService _analytics;

        public TutorialDebugAnalyticsGuard(ITutorialDebugService inner, IAnalyticsService analytics)
        {
            _inner = inner;
            _analytics = analytics;
        }

        public void RestartTutorial()
        {
            using (_analytics.Suppress()) _inner.RestartTutorial();
        }

        public void CompleteStepDebug()
        {
            using (_analytics.Suppress()) _inner.CompleteStepDebug();
        }

        public void SkipStepDebug()
        {
            using (_analytics.Suppress()) _inner.SkipStepDebug();
        }

        public void ForceStep(TutorialStepId stepId)
        {
            using (_analytics.Suppress()) _inner.ForceStep(stepId);
        }

        public void ClearProgress()
        {
            using (_analytics.Suppress()) _inner.ClearProgress();
        }
    }
}
