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
        public List<string> completedStepIds;
        public bool completed;
        public long startedAtUnixSeconds;
    }
}