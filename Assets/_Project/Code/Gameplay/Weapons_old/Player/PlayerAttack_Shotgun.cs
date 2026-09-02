
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class PlayerAttack_Shotgun : _Attack_Shooting
    {

        [Header("Настройки дробовика")]
        [SerializeField] private byte bulletInShot;
        [SerializeField] private float bulletOffset;

        
        
        
        protected override void Attack()
        {
            usedDamage = current_dmg;

            // что бы пули расходились от центра цели
            var start_offset = (bulletOffset * bulletInShot) / 2;       
            for (int i = 0; i < bulletInShot; i++)
            {
                Shot_Regular(AppConstants.layer_player_bullet, -start_offset + i * bulletOffset);
                ConsumptionAmmo();
            }
        }
    }
}