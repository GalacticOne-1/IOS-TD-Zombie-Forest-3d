using System;
using Galactic1.Code.Gameplay.Weapons.Logic;

namespace Galactic1.Code.Gameplay.Units
{
    public interface IWeaponWithEvents : IWeapon
    {
        WeaponEntity Entity { get; }
        WeaponDefinitionData Definition { get; }
        event Action OnShotLogicComplete;
        event Action<WeaponState> OnStateChanged;
        event Action OnReloadCompleted;
        event Action<int, float> OnDurabilityChanged;
        event Action<int, int> OnAmmoChanged;

        void SetVisible(bool visible);
    }
}