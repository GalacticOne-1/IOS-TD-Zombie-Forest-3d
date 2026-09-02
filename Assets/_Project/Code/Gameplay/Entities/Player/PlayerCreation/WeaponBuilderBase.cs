using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using Galactic1.Gameplay.Player;
using Galactic1.Items;

namespace Galactic1.Core.Systems.PlayerCreation
{
    public abstract class WeaponBuilderBase
    {
        private readonly IPlayerController player;

        public WeaponBuilderBase(IPlayerController player)
        {
            this.player = player;
        }

        public GameObject Apply(WeaponModule weapon, Transform weaponSocket)
        {
            if (player == null || weapon == null) 
                return null;

            if (weapon.Item.Id == null) 
                return null;


            // Load weapon prefab (e.g. Resources/Weapons/{weaponId})
            var prefab = Resources.Load<GameObject>($"{weapon.Item.PrefabPath}");
            if (prefab == null || weaponSocket == null)
            {
                Debug.LogWarning("Weapon not created!");
                return null;
            }

            var instantiated = GameObject.Instantiate(prefab, weaponSocket, false);
            
            
            // Try to find WeaponComponent and set ammo
            var weaponComp = instantiated.GetComponent<PlayerWeaponComponent>();
            if (weaponComp != null)
            {
                //weaponComp.SetAmmo(weapon.ammo);
                //player.Combat.SetEquippedWeapon(weaponComp);
            }

            return instantiated;
        }
    }
}