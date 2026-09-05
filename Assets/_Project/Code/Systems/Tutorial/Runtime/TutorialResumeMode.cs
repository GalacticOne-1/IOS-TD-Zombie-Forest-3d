using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    public enum TutorialResumeMode
    {
        /// <summary>currentStepId не завершён и безопасен в текущем домене.</summary>
        ResumeCurrent,
        /// <summary>currentStepId небезопасен — резюм со ШАГА СРАЗУ ПОСЛЕ чекпоинта
        /// (ровно один шаг вперёд по графу, не поиск).</summary>
        ResumeFromCheckpoint,
        /// <summary>currentStepId уже в completedStepIds (legacy) — резюм со следующего
        /// шага по графу от него же (тоже ровно один шаг вперёд).</summary>
        ContinueFromResolvedProgress,
        /// <summary>Ни currentStepId, ни чекпоинт не дают безопасной точки без пропуска
        /// прогрессии — единственный допустимый выход: рестарт с entry point.</summary>
        Restart
    }

    /// <summary>Диагностический контекст, заполняется ТОЛЬКО когда резолвер вынужден
    /// уйти в Restart — обязателен для видимости потери прогресса в аналитике/логах.
    /// SavedCurrentStepId/CheckpointStepId — сырые guid-строки прямо из персистентного
    /// снапшота (CGameStateTutorial), намеренно не резолвятся в TutorialStepId: это
    /// диагностика именно рассинхронизации между сохранённым состоянием и живым графом.</summary>
    public readonly struct TutorialResumeFallbackContext
    {
        public readonly string SavedCurrentStepId;
        public readonly string CheckpointStepId;
        public readonly TutorialStepDomain CurrentDomain;
        public readonly string Reason;

        public TutorialResumeFallbackContext(
            string savedCurrentStepId, string checkpointStepId, TutorialStepDomain currentDomain, string reason)
        {
            SavedCurrentStepId = savedCurrentStepId;
            CheckpointStepId = checkpointStepId;
            CurrentDomain = currentDomain;
            Reason = reason;
        }
    }

    public readonly struct TutorialResumeDecision
    {
        public readonly TutorialResumeMode Mode;
        /// <summary>Null только при Mode == Restart.</summary>
        public readonly TutorialStepId StepId;
        /// <summary>Заполнен только при Mode == Restart.</summary>
        public readonly TutorialResumeFallbackContext? FallbackContext;

        private TutorialResumeDecision(TutorialResumeMode mode, TutorialStepId stepId, TutorialResumeFallbackContext? fallbackContext)
        {
            Mode = mode;
            StepId = stepId;
            FallbackContext = fallbackContext;
        }

        public static TutorialResumeDecision ResumeCurrent(TutorialStepId stepId)
            => new(TutorialResumeMode.ResumeCurrent, stepId, null);

        public static TutorialResumeDecision ResumeFromCheckpoint(TutorialStepDefinition step)
            => new(TutorialResumeMode.ResumeFromCheckpoint, step.stepId, null);

        public static TutorialResumeDecision ContinueFromResolvedProgress(TutorialStepDefinition step)
            => new(TutorialResumeMode.ContinueFromResolvedProgress, step.stepId, null);

        public static TutorialResumeDecision Restart(TutorialResumeFallbackContext context)
            => new(TutorialResumeMode.Restart, null, context);
    }
}
