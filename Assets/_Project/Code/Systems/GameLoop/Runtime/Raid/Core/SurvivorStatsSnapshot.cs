using System.Collections.Generic;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Snapshot текущих статов survivor для рейда.
    /// </summary>
    public sealed class SurvivorStatsSnapshot
    {
        public IReadOnlyDictionary<StatId, float> BaseStats { get; }
        public IReadOnlyDictionary<StatId, float> CurrentStats { get; }

        public SurvivorStatsSnapshot(
            Dictionary<StatId, float> baseStats,
            Dictionary<StatId, float> currentStats)
        {
            BaseStats = baseStats;
            CurrentStats = currentStats;
        }
    }
}