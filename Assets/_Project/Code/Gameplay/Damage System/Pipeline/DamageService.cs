
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Damage.Pipeline;
using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Damage
{
    public static class DamageService
    {
        private static readonly DamagePipeline _pipeline =
            new DamagePipeline()
                .Add(new DeadCheckStep())
                .Add(new BuffModifierStep())
                .Add(new BodyPartModifierStep())
                .Add(new ArmorReductionStep())
                .Add(new ApplyDamageStep())
                .Add(new RetaliationStep(ServiceLocator.Current.Get<CombatEventService>()))
                .Add(new ArmorDurabilityStep());

        public static DamageResult ApplyDamage(
            IUnitSceneContext attacker,
            IUnitSceneContext target,
            float damage,
            DamageType type,
            HitInfo hitInfo)
        {
            var ctx = new DamageContext(
                attacker,
                target,
                damage,
                type,
                hitInfo);

            return _pipeline.Execute(ctx);
        }
    }
}