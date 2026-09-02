using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    public class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private float attackCooldown = 0.8f;
        private float nextAttackTime;
        private PlayerWeaponComponent equippedWeapon;

        public void SetEquippedWeapon(PlayerWeaponComponent weapon)
        {
            equippedWeapon = weapon;
        }

        public void TryAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackCooldown;
            PerformAttack();
        }

        private void PerformAttack()
        {
            Debug.Log("[PlayerCombat] Attack performed");
            // TODO: raycast / hit detection / animation triggers
            if (equippedWeapon != null)
            {
                // example: use ammo if needed
            }
        }
    }
}