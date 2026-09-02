
namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_SC : _Attack_Shooting_SC
    {
        /*
         *      Без пуль
         */

        
        
        protected override void Attack()
        {
            usedDamage = current_dmg;
            Shot_Regular(AppConstants.layer_player_bullet);
            
            ConsumptionAmmo();
            if (shootFx.shellParticles)
                shootFx.shellParticles.Emit(1);
        }
    }
}