
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_Shotgun_RAY : _Attack_Shooting_RAY
    {
        /*
         *      Дробовик по лучам
         */
        
        
        [Header("Настройки дробовика")]
        [SerializeField] private byte bulletInShot;
        [SerializeField] private float rayOffset;

        private Vector3 _rayAngle;
        
        
        protected override void Attack()
        {
            usedDamage = current_dmg;

            // #1 данные для центрального луча
            direction = _attack.Entity.Target.ITarget.HitCoord() - modeShell.bar.transform.position;
            //distance = Vector2.Distance(_attack.unit._target.ITarget.HitCoord(), modeShell.bar.transform.position);
            

            // #2 что бы лучи расходились от центра цели
            var start_offset = (rayOffset * bulletInShot) / 2;       
            for (int i = 0; i < bulletInShot; i++)
            {
                _rayAngle.z = -start_offset + i * rayOffset;
                
                //Debug.DrawRay(modeShell.bar.transform.position,
                //Quaternion.Euler(_rayAngle) * direction.normalized * distance, Color.red, 1);

                Shot_Multiple(_rayAngle);
            }
            ConsumptionAmmo();
        }
    }
}