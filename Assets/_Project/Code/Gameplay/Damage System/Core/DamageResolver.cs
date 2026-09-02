using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Damage
{
    public static class DamageResolver
    {
        public static DamageResult Apply(
            DamageReceiverProxy receiver,
            IUnitSceneContext attacker,
            float damage,
            DamageType type,
            HitInfo hitInfo)
        {
            if (receiver == null)
                return new DamageResult();

            // ✅ ЮНИТ
            if (receiver.Unit != null)
            {
                return DamageService.ApplyDamage(
                    attacker,
                    receiver.Unit,
                    damage,
                    type,
                    hitInfo);
            }

            // ✅ ENVIRONMENT
            if (receiver.Damageable != null)
            {
                receiver.Damageable.ApplyDamage(damage);
            }

            return new DamageResult();
        }
    }
}