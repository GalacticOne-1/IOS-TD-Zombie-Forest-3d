using System.Collections.Generic;

namespace Galactic1.Code.Systems.Raid.Buildings
{
    /// <summary>
    /// Immutable snapshot статов здания.
    /// </summary>
    public sealed class FacilityStatsSnapshot
    {
        public readonly Dictionary<StatId, float> BaseStats;
        public readonly Dictionary<StatId, float> CurrentStats;

        public FacilityStatsSnapshot(
            Dictionary<StatId, float> baseStats,
            Dictionary<StatId, float> currentStats)
        {
            BaseStats = baseStats;
            CurrentStats = currentStats;
        }
    }
}