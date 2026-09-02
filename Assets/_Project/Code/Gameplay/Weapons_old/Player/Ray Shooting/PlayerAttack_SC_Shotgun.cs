
using Galactic1.Code.Gameplay.Damage;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_SC_Shotgun : _Attack_Shooting_SC
    {
        private float angle = 30;
        
        protected override void Attack()
        {
            usedDamage = current_dmg;
            ConsumptionAmmo();
            if (shootFx.shellParticles)
                shootFx.shellParticles.Emit(1);
            
            Vector3 origin = modeShell.bar.transform.position;

            // --- направление на цель (нормализуем и обнуляем Z)
            Vector3 direction = Vector3.zero;
            ;// (_attack.Entity.Target.ITarget.Hp.HealthComponentCollider.transform.position - origin);
            direction.z = 0;
            direction.Normalize();
            
            ShotgunService.FireShotgun(
                origin,
                direction,
                angle,
                weaponSetup.rangeAttack,
                usedDamage,
                5,
                1 << AppConstants.layer_detect_enemies_gr,
                _attack.Entity);
            
        }
        
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var origin = modeShell.bar.transform.position;
            var target = _attack && _attack.Entity.Target.ITarget != null
                ? _attack.Entity.Target.ITarget.tr
                : null;

            if (target != null)
                ShotgunService.DrawDebugConeForTurret(
                    origin,
                    target,
                    angle,
                    weaponSetup.rangeAttack,
                    new Color(1f, 0.5f, 0f, 0.15f),
                    Color.red,
                    1 << AppConstants.layer_detect_enemies_gr
                );
        }
#endif
        
    }
}