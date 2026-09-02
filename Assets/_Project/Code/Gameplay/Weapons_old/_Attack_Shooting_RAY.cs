
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _Attack_Shooting_RAY : _Attack
    {
        /*
         *      ! For player !
         */
        
        
        protected float distance;
        
        
        
        protected void Shot_Regular(byte layer)
        {
            // _attack.Entity._target.ITarget.HealthModule.OnHit(new CDamageSetup()
            //     {
            //         status = EStatusDamage.bullet,
            //         damage = usedDamage,
            //         crit = false,
            //         
            //     },
            //     new CHitEvent()
            //     {
            //         attacker = _attack.Entity.gameObject,
            //     });
            // SpawnBites();
        }

        protected void Shot_Multiple(Vector3 angle)
        {
            // var hit = modeShell.bar.transform.position.Ray2d(
            //     Quaternion.Euler(angle) * direction.normalized,
            //     weaponSetup.rangeAttack,
            //     1 << Globals.layer_detect_enemies_gr);
            //
            // // если луч наткнулся на объект
            // if (hit)
            // {
            //     DLog.Alert($"_Attack_Shooting_RAY : ray hit {hit.transform}", EDlogColor.ORANGE);
            //     if (hit.transform.GetComponent<HealthComponentCollider>() == null)
            //     {
            //         FBA.CRASH("_Attack_Shooting_RAY : raycast get object with a null <HealthComponentCollider>");
            //         return;
            //     }
            //
            //     // set damage
            //     hit.transform.GetComponent<HealthComponentCollider>().GetControlller().HealthModule.OnHit(new CDamageSetup()
            //         {
            //             status = EStatusDamage.bullet,
            //             damage = usedDamage,
            //         },
            //         new CHitEvent()
            //         {
            //             attacker = _attack.Entity.gameObject,
            //         });
            //     SpawnBites();
            // }
        }

        protected void Shot_Across(byte maxTargets)
        {
            // var hit = modeShell.bar.transform.position.GetObjInRay(
            //     direction.normalized,
            //     weaponSetup.rangeAttack + weaponSetup.rangeAttack / 2,      // что бы урон могли получить еще дальше от края атаки
            //     1 << Globals.layer_detect_enemies_gr);
            //
            // if (hit != null)
            // {
            //     var l = hit.Length;
            //     for (int i = 0; i < l; i++)
            //     {
            //         if (i >= maxTargets)        // нанесли урон макс кол-ву юнитов 
            //             break;
            //         
            //         // set damage
            //         hit[i].transform.GetComponent<HealthComponentCollider>().GetControlller().HealthModule.OnHit(new CDamageSetup()
            //             {
            //                 status = EStatusDamage.bullet,
            //                 damage = usedDamage,
            //             },
            //             new CHitEvent()
            //             {
            //                 attacker = _attack.Entity.gameObject,
            //             });
            //         SpawnBites();
            //     }
            // }
            
        }


        /// <summary>
        /// Спавн кусков от попадания по цели
        /// </summary>
        void SpawnBites()
        {
            
        }
    }
}