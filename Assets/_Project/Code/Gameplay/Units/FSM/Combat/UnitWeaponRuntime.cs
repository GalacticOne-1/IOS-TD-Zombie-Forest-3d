using System;
using Galactic1.Code.Gameplay.Units;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-хранилище оружия юнита.
    /// Единственный источник истины для UI.
    /// </summary>
    public sealed class UnitWeaponRuntime : IUnitWeaponRuntime
    {
        private IWeaponWithEvents _current;

        public IWeaponWithEvents CurrentWeapon => _current;

        public event Action<IWeaponWithEvents> OnWeaponChanged;

        // =========================
        // API (вызывается bridge/scene)
        // =========================

        public void SetWeapon(IWeaponWithEvents newWeapon)
        {
            if (_current == newWeapon)
                return;

            _current = newWeapon;

            OnWeaponChanged?.Invoke(_current);
        }

        public void ClearWeapon()
        {
            if (_current == null)
                return;

            _current = null;
            OnWeaponChanged?.Invoke(null);
        }
    }
}