using Galactic1.Code.GameDatabase.Registries;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Core;

namespace Galactic1.RaidLoot.Services
{
    public static class LootSeedProvider
    {
        /// <summary>
        /// Seed для конкретного контейнера.
        /// Уникален по: container + location + day + openCount.
        /// </summary>
        public static int ComputeSeed(
            string id,
            LocationId locationId,
            int dayNumber,
            int openCount)
        {
            var key = $"{id}|{locationId.Guid}|{dayNumber}|{openCount}";
            return StableHash.Compute(key);
        }

        // Task 6: отдельный метод для location-guaranteed лута.
        // Использует фиксированный префикс "loc_guar" — никогда не пересечётся
        // с container seed (там первый сегмент — containerId.Guid).
        /// <summary>
        /// Seed для гарантированного лута локации.
        /// Генерируется один раз за рейд, не зависит от контейнеров.
        /// </summary>
        public static int ComputeLocationGuaranteedSeed(
            LocationId locationId,
            int dayNumber)
        {
            var key = $"loc_guar|{locationId.Guid}|{dayNumber}";
            return StableHash.Compute(key);
        }

        // Устаревшая перегрузка — оставлена для обратной совместимости.
        // Используйте ComputeLocationGuaranteedSeed вместо неё.
        [System.Obsolete("Use ComputeLocationGuaranteedSeed(locationId, dayNumber) instead.")]
        public static int ComputeSeed(
            LocationId locationId,
            int dayNumber,
            int openCount)
        {
            var key = $"location_guaranteed|{locationId.Guid}|{dayNumber}|{openCount}";
            return StableHash.Compute(key);
        }
    }
}