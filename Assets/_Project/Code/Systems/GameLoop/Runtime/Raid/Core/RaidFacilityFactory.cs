using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Raid.Buildings
{
    /// <summary>
    /// Создает immutable snapshot боевого сооружения.
    /// </summary>
    public sealed class RaidFacilityFactory
    {
        public RaidFacilitySnapshot Create(CombatFacilityRuntime runtime)
        {
            var baseStats = new Dictionary<StatId, float>();

            foreach (var s in runtime.Stats.GetBaseStats)
                baseStats[s.Key] = s.Value;

            var currentStats = new Dictionary<StatId, float>();

            foreach (var s in runtime.Stats.CurrentStats_)
                currentStats[s.Key] = s.Value.Value;

            var statsSnapshot = new FacilityStatsSnapshot(
                baseStats,
                currentStats);

            return new RaidFacilitySnapshot(
                runtime.Id,
                runtime.ConfigId,
                runtime.Config,
                runtime.HealthModule,
                statsSnapshot,
                runtime.Position,
                runtime.Rotation,
                runtime.Level);
        }
    }
}