
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.GameLoop;

namespace Galactic1.Code.UI.RaidReport
{
    public static class CampDefenseReportMapper
    {
        public static RaidReportData Build(GameLoopContext context)
        {

            var result = context.Proxy.LastRaidResult;
            
            // loot
            var lossResults = new List<RaidLossResult>();
            var l = result.ResourcesLost.Count;
            for (int i = 0; i < l; i++)
            {
                var item = result.ResourcesLost[i];
                if (GameContent.ResolveItem(item.ConfigId.Value, out var config))
                {
                    lossResults.Add(new RaidLossResult
                    {
                        Item = config,
                        Amount = item.Amount.Value,
                    });
                }
            }
            
            return new RaidReportData
            {
                Survivors = RaidReportUtility.Survivors(context.StrategicSquadUnits),
                CampSurvivors = RaidReportUtility.Survivors(context.CampUnits),
                Loot = null,
                AdBonusAvail = false,
                ResourcesLost = lossResults
            };
        }
    }
}