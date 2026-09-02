using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_Regular : _Attack_Shooting
    {

        protected override void Attack()
        {
            usedDamage = current_dmg;
            Shot_Regular(AppConstants.layer_player_bullet);
            ConsumptionAmmo();
        }
    }
}