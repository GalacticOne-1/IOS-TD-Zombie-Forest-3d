
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Меняет AnimatorOverrideController при смене оружия.
    /// Вызывается только из UnitAnimationController.
    /// </summary>
    public sealed class WeaponAnimSwitcher : MonoBehaviour, IWeaponAnimatorSwitcher
    {
        private WeaponAnimLibrary library;
        private AnimatorBridge _bridge;
        private WeaponType _currentWeapon = WeaponType.Unarmed;
        
        public WeaponAnimLibrary AnimLibrary { set => library = value; }
        
        private bool _isDead;

        private void Awake()
            => _bridge = GetComponent<AnimatorBridge>();

        public void SetWeapon(WeaponType type)
        {
            if (_isDead || type == _currentWeapon) return;
            _currentWeapon = type;

            var controller = library.GetController(type);
            _bridge.SetOverrideController(controller);
        }

        public WeaponType CurrentWeapon => _currentWeapon;
        
        public void Die() => _isDead = true;
    }
}