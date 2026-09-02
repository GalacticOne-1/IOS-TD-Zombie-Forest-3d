using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_Melee : _Attack_Melee
    {
        protected override void Attack()
        {
            usedDamage = current_dmg;
            Hit();
            ConsumptionAmmo();
        }
    }
}