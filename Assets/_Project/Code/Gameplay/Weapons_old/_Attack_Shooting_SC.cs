
using Galactic1.Code.Gameplay.Damage;

namespace Galactic1.AbstractFactory
{
    public abstract class _Attack_Shooting_SC : _Attack
    {
        
        
        protected void Shot_Regular(byte layer)
        {
            // ServiceLocator.Current.Get<DamageSystem>().ApplyDamage(new DamageEvent()
            // {
            //     Attacker = _attack.Entity,
            //     Target = _attack.Entity.Target.ITarget.Obj.GetComponent<_Entity>(),
            //     Amount = usedDamage,
            //     Type = DamageType.Bullet
            // });
        }
        
        protected void Shot_Regular(byte layer, float rotateOffset)
        {
            
        }
        
        
        protected void Shot_AoE(byte layer)
        {
            
        }
    }
}