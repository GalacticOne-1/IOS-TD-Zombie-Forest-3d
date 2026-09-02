using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Scene.Units
{
    /// <summary>
    /// Связывает WeaponSlot (Scene) с UnitWeaponRuntime.
    /// </summary>
    [RequireComponent(typeof(WeaponSlot))]
    public sealed class WeaponRuntimeBinder : MonoBehaviour
    {
        private WeaponSlot _weaponSlot;
        private IUnitRuntime _unitRuntime;

        // =========================
        // Init
        // =========================
        public void Initialize(IUnitRuntime runtime)
        {
            _unitRuntime = runtime;
            _weaponSlot = GetComponent<WeaponSlot>();

            // начальное состояние
            SyncCurrent();

            // подписки
            _weaponSlot.OnWeaponMounted += OnMounted;
            _weaponSlot.OnWeaponUnmounted += OnUnmounted;
        }

        private void OnDestroy()
        {
            if (_weaponSlot != null)
            {
                _weaponSlot.OnWeaponMounted -= OnMounted;
                _weaponSlot.OnWeaponUnmounted -= OnUnmounted;
            }
        }

        // =========================
        // Events
        // =========================
        private void OnMounted(WeaponHandle handle)
        {
            SyncCurrent();
        }

        private void OnUnmounted()
        {
            _unitRuntime.Weapon.ClearWeapon();
        }

        private void SyncCurrent()
        {
            _unitRuntime.Weapon.SetWeapon(_weaponSlot.CurrentWeapon);
        }

    }
}