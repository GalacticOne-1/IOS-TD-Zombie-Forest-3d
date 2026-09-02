

namespace Galactic1.AbstractFactory
{
    public class EnemyAttack_Melee : _Attack_Melee
    {

        protected override void Attack()
        {
            usedDamage = current_dmg;
            Hit();
            ConsumptionAmmo();
        }
    }
}