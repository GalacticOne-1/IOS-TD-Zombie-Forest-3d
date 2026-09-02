
namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_RAY : _Attack_Shooting_RAY
    {
        /*
         *      Без пуль
         */
        
        
        protected override void Attack()
        {
            usedDamage = current_dmg;
            Shot_Regular(AppConstants.layer_player_bullet);
            ConsumptionAmmo();
        }
    }
}