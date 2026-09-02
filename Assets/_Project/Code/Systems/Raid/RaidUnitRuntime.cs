
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    public sealed partial class RaidUnitRuntime : IRaidResolvable
    {
        public void ApplyToMeta(object metaRuntime)
        {
            if (metaRuntime is not UnitRuntime meta)
                return;

            // 1️⃣ только HP
            meta.Stats.SetStat(StatId.Health, Stats.CurrentHP);

            // 2️⃣ инвентарь + экипировка
            RaidInventorySyncService.RestoreFromSnapshot(
                _inventorySource.Equipment,
                meta.Sources[0]
            );

            // meta.EquipmentService.RestoreFromSnapshot(
            //     EquipmentService.CreateSnapshot()
            // );
        }
    }
}