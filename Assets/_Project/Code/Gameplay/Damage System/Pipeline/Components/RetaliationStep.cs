using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Ответный урон атакующему, если цель — сооружение с пассивным
    /// уроном (колья, колючая проволока). Срабатывает только на реальный
    /// контакт (мили-атака зомби), т.к. Attacker != null только там.
    ///
    /// Не модифицирует ctx.Damage — чистый побочный эффект,
    /// поэтому return true всегда (не блокирует пайплайн).
    /// </summary>
    public sealed class RetaliationStep : IDamageStep
    {
        private readonly CombatEventService _combatEventService;

        public RetaliationStep(CombatEventService combatEventService)
        {
            _combatEventService = combatEventService;
        }


        public bool Process(DamageContext ctx)
        {
            if (ctx.Attacker == null)
                return true;

            if (ctx.Target?.RuntimeBase is not IRetaliatingFacility facility)
                return true;

            if (!facility.TryGetRetaliationDamage(out var retaliationDamage))
                return true;

            var result = DamageService.ApplyDamage(
                null, // ответный удар без атакующего — reентерабельно безопасно
                ctx.Attacker,
                retaliationDamage,
                DamageType.Hit,
                ctx.HitInfo);

            _combatEventService.RaiseHit(
                ctx.Attacker,
                result,
                ctx.HitInfo,
                Vector3.zero);
            
#if UNITY_EDITOR
            DLog.Alert($"<retaliatory damage> {ctx.Target} / {ctx.Damage} => {ctx.Target.Stats.Get(StatId.Health)}", EDlogColor.ORANGE);
#endif

            return true;
        }
    }
}