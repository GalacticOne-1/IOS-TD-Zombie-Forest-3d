using Galactic1.Code.Systems.Runtime;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Raid
{
    public sealed partial class RaidCampRuntime : IRaidResolvable
    {
        public void ApplyToMeta(object metaRuntime)
        {
            if (metaRuntime is not CampRuntime meta)
                return;

            // инвентарь лагеря
            RaidInventorySyncService.RestoreFromSnapshot(
                Sources.Cargo,
                meta.GetInventory(StorageType.Regular)
            );
        }
    }
}