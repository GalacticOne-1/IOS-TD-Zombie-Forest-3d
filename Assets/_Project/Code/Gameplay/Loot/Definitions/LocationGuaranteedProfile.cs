
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Definition
{
    /// <summary>
    /// Runtime-слепок LocationGuaranteedProfileConfig.
    /// Иммутабелен. Нет зависимостей от Unity.
    /// Хранится в LocationContext, читается LootGenerationService.
    /// </summary>
    public sealed class LocationGuaranteedProfile
    {
        public LocationId LocationId { get; }
 
        public IReadOnlyList<LocationGuaranteedEntry> Entries { get; }
 
        public LocationGuaranteedProfile(
            LocationId locationId,
            IReadOnlyList<LocationGuaranteedEntry> entries)
        {
            LocationId = locationId;
            Entries    = entries;
        }
    }
}