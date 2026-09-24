using System;
using System.Collections.Generic;

namespace Galactic1
{
    [Serializable]
    public struct CGameStateTutorial
    {
        public string campaignId;
        public string currentStepId;
        public string checkpointStepId;
        public List<string> completedStepIds;        // прогресс ТЕКУЩЕЙ кампании, сбрасывается в ResetProgression
        public List<string> allCompletedStepIds;     // NEW: накопительно по всем кампаниям, не сбрасывается при смене кампании
        public List<string> claimedRewardStepIds;
        public bool completed;
        public long startedAtUnixSeconds;
    }
}