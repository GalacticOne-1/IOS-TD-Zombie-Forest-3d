
using Galactic1.Code.Gameplay.Damage;


namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_SC_Random : _Attack_Shooting_SC
    {
        /*
         *      Без пуль, атакует рандомно 1 цель в радиусе от основной
         */
        
        
        protected override void Attack()
        {
            usedDamage = current_dmg;
            //Shot_Regular(Globals.layer_player_bullet);
            ConsumptionAmmo();
            if (shootFx.shellParticles)
                shootFx.shellParticles.Emit(1);

            // ExplosionService.ApplySplashDamage(
            //     _attack.Entity._target.ITarget.tr.position,
            //     2,
            //     usedDamage,
            //     _attack.Entity._target.ITarget.Obj.GetComponent<_Entity>(),
            //     1 << Globals.layer_detect_enemies_gr);

            // var target = TargetFinderService.FindTarget(
            //     _attack.Entity.Target.ITarget.tr.position,
            //     1.5f,
            //     1 << AppConstants.layer_detect_enemies_gr,
            //     TargetSelectionMode.Random);
            // if(target)
            // {
            //     ServiceLocator.Current.Get<DamageSystem>().ApplyDamage(new DamageEvent()
            //     {
            //         Attacker = _attack.Entity,
            //         Target = target.GetComponent<IHealthComponentCollider>().GetControlller(),
            //         Amount = usedDamage,
            //         Type = DamageType.Bullet
            //     });
            // }
        }
        
        
    }
}