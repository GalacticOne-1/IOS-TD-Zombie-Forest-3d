namespace Galactic1.Code.Systems.Tutorial.Analytics
{
    /// <summary>
    /// Наблюдательный, неблокирующий репортер. TutorialService всегда работает
    /// корректно даже с NullTutorialAnalytics — прогрессия тутора никогда не зависит
    /// от успеха аналитики.
    /// </summary>
    public interface ITutorialAnalytics
    {
        void TutorialStarted(string campaignId);
        void TutorialResumed(string campaignId);

        /// <summary>Резолвер был вынужден полностью перезапустить кампанию — прогресс
        /// потерян не по вине игрока. Без этого события tutorial_step_started=entry
        /// в аналитике неотличим от обычного первого запуска.</summary>
        void TutorialResumeFallback(string campaignId, string savedCurrentStepId, string checkpointStepId,
            string currentDomain, string reason);

        void StepStarted(string campaignId, string chapterId, string stepId, int stepIndex);
        void StepCompleted(string campaignId, string chapterId, string stepId, int stepIndex);
        void StepSkipped(string campaignId, string chapterId, string stepId, int stepIndex);
        void CheckpointReached(string campaignId, string stepId);
        void TutorialCompleted(string campaignId);
        void TutorialAbandoned(string campaignId, string lastStepId);
    }

    public sealed class NullTutorialAnalytics : ITutorialAnalytics
    {
        public void TutorialStarted(string campaignId) { }
        public void TutorialResumed(string campaignId) { }
        public void TutorialResumeFallback(string campaignId, string savedCurrentStepId, string checkpointStepId,
            string currentDomain, string reason) { }
        public void StepStarted(string campaignId, string chapterId, string stepId, int stepIndex) { }
        public void StepCompleted(string campaignId, string chapterId, string stepId, int stepIndex) { }
        public void StepSkipped(string campaignId, string chapterId, string stepId, int stepIndex) { }
        public void CheckpointReached(string campaignId, string stepId) { }
        public void TutorialCompleted(string campaignId) { }
        public void TutorialAbandoned(string campaignId, string lastStepId) { }
    }
}
