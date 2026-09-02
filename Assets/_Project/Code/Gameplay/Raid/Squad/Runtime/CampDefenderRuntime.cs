using System.Collections.Generic;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    public sealed class CampDefenderRuntime : RaidUnitRosterRuntime
    {
        public CampDefenderRuntime(
            List<UnitRuntime> metaUnits,
            InventoryAccessService access,
            PlayerArchetypeConfig playerCfg,
            SurvivorConsumptionConfig consumptionConfig)
            : base(BuildUnits(metaUnits, access, playerCfg, consumptionConfig))
        {
        }

        public static List<RaidUnitRuntime> BuildUnits(
            List<UnitRuntime> metaUnits, 
            InventoryAccessService access,
            PlayerArchetypeConfig playerCfg,
            SurvivorConsumptionConfig consumptionConfig)
        {
            
            var factory = new RaidSurvivorFactory();
            var result = new List<RaidUnitRuntime>();

            // === создаем копии UnitRuntime, живут только в рейде
            foreach (var u in metaUnits)
            {
                var snapshot = factory.Create(u, access, playerCfg);
                result.Add(new RaidUnitRuntime(snapshot, consumptionConfig, true));
            }

            return result;
        }
    }
}