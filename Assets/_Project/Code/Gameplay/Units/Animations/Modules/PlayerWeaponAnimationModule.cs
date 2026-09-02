using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapon.Animation
{
    /// <summary>
    /// Weapon animation implementation for armed survivor units.
    /// </summary>
    public sealed class PlayerWeaponAnimationModule : MonoBehaviour, IWeaponAnimationModule
    {
        private WeaponRigController _rigController;
        private WeaponSlot _weaponSlot;

        public void Initialize()
        {
            _rigController = GetComponent<WeaponRigController>();
            _weaponSlot = GetComponent<WeaponSlot>();
        }

        public void SetWeaponVisible(bool visible)
        {
            _weaponSlot?.CurrentWeapon?.SetVisible(visible);
        }

        public void SetRigEnabled(bool enabled)
        {
            _rigController?.SetRigEnabled(enabled);
        }
    }
}