
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.Raid
{
    public sealed partial class RaidVehicleRuntime : IRaidResolvable
    {

        public void ApplyToMeta(object metaRuntime)
        {
            if (metaRuntime is not TransportRuntime meta)
                return;

            // 1️⃣ модули / оборудование
            RaidInventorySyncService.RestoreFromSnapshot(
                Sources.Equipment,
                meta.Sources[0]
            );
            
            // 2️⃣ грузовой инвентарь
            RaidInventorySyncService.RestoreFromSnapshot(
                Sources.Cargo,
                meta.Sources[1]
            );

            
        }
    }
}