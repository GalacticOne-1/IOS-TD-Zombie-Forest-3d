
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_Marksman_RAY : _Attack_Shooting_RAY
    {
        /*
         *      Снайперка по лучу, до нескольких целей
         */
        
        
        [Header("Кол-во целей за один выстрел")]
        [SerializeField] private byte maxTargets;

        
        protected override void Attack()
        {
            usedDamage = current_dmg;

            direction = _attack.Entity.Target.ITarget.HitCoord() - modeShell.bar.transform.position;
            Shot_Across(maxTargets);
            ConsumptionAmmo();
        }
    }
}