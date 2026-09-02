
using System.Collections.Generic;

namespace Galactic1.Code.Core
{
    [System.Serializable]
    public class RaidResultData
    {
        public bool IsSuccess { get;  set; }
        public int KilledEnemies { get;  set; }
        public int ExperienceGained { get; set; }
        public bool MainBuildingDestroyed { get; set; }
        
        


        public List<RaidRewardLootData> LootReceived { get;  set; }
        public List<RaidPenaltyLossData> ResourcesLost { get; set; }
    }
}