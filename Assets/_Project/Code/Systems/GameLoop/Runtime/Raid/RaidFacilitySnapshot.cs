using UnityEngine;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Raid.Buildings
{
    /// <summary>
    /// Immutable snapshot боевого сооружения.
    /// Используется исключительно внутри Camp Defense.
    /// </summary>
    public sealed class RaidFacilitySnapshot
    {
        public string FacilityId { get; }

        public string ConfigId { get; }

        public FacilityModule FacilityModule { get; }

        public BuildingHealthModule HealthModule { get; }

        public FacilityStatsSnapshot StatsSnapshot { get; }

        public Vector2Int Position { get; }

        public int Rotation { get; }

        public int Level { get; }

        public RaidFacilitySnapshot(
            string facilityId,
            string configId,
            FacilityModule facilityModule,
            BuildingHealthModule healthModule,
            FacilityStatsSnapshot statsSnapshot,
            Vector2Int position,
            int rotation,
            int level)
        {
            FacilityId = facilityId;
            ConfigId = configId;
            FacilityModule = facilityModule;
            HealthModule = healthModule;
            StatsSnapshot = statsSnapshot;
            Position = position;
            Rotation = rotation;
            Level = level;
        }
    }
}