
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class ArmorReductionStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            if (ctx.Target == null)
                return true;

            float armor = ctx.Target.Stats.Get(StatId.Armor).Value;

            if (armor <= 0)
                return true;

            // 🔹 применяем penetration (игнор части брони)
            float effectiveArmor = armor * (1f - ctx.ArmorPenetration);

            // 🔹 студийная формула (diminishing returns)
            float reduction = effectiveArmor / (effectiveArmor + 30f);
            
            // минимум 2%
            if (effectiveArmor > 0f)
                reduction = Mathf.Max(reduction, 0.02f);

            float final = ctx.Damage * (1f - reduction);

            ctx.Damage = Mathf.Max(final, 0);

            return true;
        }
    }
}