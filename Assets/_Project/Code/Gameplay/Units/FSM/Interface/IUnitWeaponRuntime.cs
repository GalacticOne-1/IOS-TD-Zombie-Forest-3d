using System;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapons.Logic;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-представление оружия юнита.
    /// НЕ зависит от сцены.
    /// </summary>
    public interface IUnitWeaponRuntime
    {
        IWeaponWithEvents CurrentWeapon { get; }

        event Action<IWeaponWithEvents> OnWeaponChanged;

        void SetWeapon(IWeaponWithEvents weapon);
        void ClearWeapon();
    }
}