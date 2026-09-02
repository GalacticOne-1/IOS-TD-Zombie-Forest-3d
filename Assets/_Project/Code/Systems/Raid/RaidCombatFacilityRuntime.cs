using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public partial class RaidCombatFacilityRuntime : IRaidResolvable
    {
        public void ApplyToMeta(object metaRuntime)
        {
            if (metaRuntime is not CombatFacilityRuntime meta)
                return;

            meta.Stats.SetStat(
                StatId.Health,
                Stats.CurrentHP);
        }
    }
}