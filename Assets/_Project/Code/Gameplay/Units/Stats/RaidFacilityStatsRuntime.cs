using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Stats;

namespace Galactic1.Code.Systems.Raid.Buildings
{
    /// <summary>
    /// Runtime статов сооружения внутри Camp Defense.
    /// Не связан с FacilityProxy.
    /// </summary>
    public sealed class RaidFacilityStatsRuntime : StatsRuntimeBase
    {
        public RaidFacilityStatsRuntime(
            string owner,
            Dictionary<StatId, float> baseStats,
            Dictionary<StatId, float> savedCurrent,
            IEquipmentStatsProvider equipmentStatsProvider)
            : base(owner, baseStats, equipmentStatsProvider)
        {
            foreach (var kv in savedCurrent)
                SetIfExists(kv.Key, kv.Value);

            ActivateLive();
        }
    }
}