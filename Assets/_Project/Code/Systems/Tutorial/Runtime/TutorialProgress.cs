using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Иммутабельный снэпшот прогресса тутора для внешнего потребления
    /// (TutorialService.GetProgress(), Debugger, аналитика).</summary>
    public sealed class TutorialProgress
    {
        public readonly string CampaignId;
        public readonly string CurrentChapterId;
        public readonly string CurrentStepId;
        public readonly string CheckpointStepId;
        public readonly bool IsCompleted;
        public readonly bool IsActive;
        public readonly IReadOnlyList<string> CompletedStepIds;

        public TutorialProgress(
            string campaignId, string currentChapterId, string currentStepId, string checkpointStepId,
            bool isCompleted, bool isActive, IReadOnlyList<string> completedStepIds)
        {
            CampaignId = campaignId;
            CurrentChapterId = currentChapterId;
            CurrentStepId = currentStepId;
            CheckpointStepId = checkpointStepId;
            IsCompleted = isCompleted;
            IsActive = isActive;
            CompletedStepIds = completedStepIds;
        }
    }
}
