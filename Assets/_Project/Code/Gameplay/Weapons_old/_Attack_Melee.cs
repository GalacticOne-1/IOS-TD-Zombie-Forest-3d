
using Galactic1.Code.Gameplay.Damage;

namespace Galactic1.AbstractFactory
{
    public abstract class _Attack_Melee : _Attack
    {
        /*
         *      Ближний бой
         */


        public void Hit()
        {
             //_attack.Entity._target.ITarget.HealthModule.OnHit(new CDamageSetup()
            // {
            //     status = EStatusDamage.hit,
            //     piercing = weaponSetup.piercing,
            //     damage = usedDamage,
            // }, 
            //     new CHitEvent()
            // {
            //     attacker = _attack.Entity.gameObject,
            // });
            // ServiceLocator.Current.Get<DamageSystem>().ApplyDamage(new DamageEvent()
            // {
            //     Attacker = _attack.Entity,
            //     Target = _attack.Entity.Target.ITarget.Obj.GetComponent<_Entity>(),
            //     Amount = usedDamage,
            //     Type = DamageType.Hit
            // });

            // _attack.Entity.Target.ITarget.Hp.OnHit(new()
            // {
            //     Attacker = _attack.Entity.UnitRuntime,
            //     Target = _attack.Entity.UnitRuntime,
            //     Amount = usedDamage,
            //     Type = DamageType.Hit
            // });
        }
        
    }
}