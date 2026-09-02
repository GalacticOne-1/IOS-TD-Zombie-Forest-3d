using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Износ брони на основе полученного урона
    /// </summary>
    public sealed class ArmorDurabilityStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            if (ctx.Target == null ||
                ctx.Target is not ISceneUnit unit) // броня только у юнитов игрока
                return true;

            if (ctx.FinalDamage <= 0)
                return true;

            var equipment = unit.EquipmentStatsProvider;
            if (equipment == null)
                return true;

            equipment.ApplyDurabilityDamage(ctx.FinalDamage);

            return true;
        }
    }
}