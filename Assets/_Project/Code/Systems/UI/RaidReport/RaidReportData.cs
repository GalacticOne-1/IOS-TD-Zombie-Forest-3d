using System.Collections.Generic;
using Galactic1.Code.Core;

namespace Galactic1.Code.UI.RaidReport
{
    public class RaidReportData
    {
        public string LocationTitle;
        public RaidResultProxy RaidResult;
        
        public List<RaidSurvivorResult> Survivors;
        public List<RaidSurvivorResult> CampSurvivors;
        public List<RaidLootResult> Loot;
        public int BonusLootCount;

        public bool LootEmpty;
        public bool CargoAvail;
        public bool AdBonusAvail;
        public bool AdBonusApplied;
        
        // === Camp Defense — что было потеряно при поражении ===
        public List<RaidLossResult> ResourcesLost;
        public bool HasResourcesLost => ResourcesLost is { Count: > 0 };
    }
}