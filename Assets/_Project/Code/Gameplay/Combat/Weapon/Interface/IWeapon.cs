
using Galactic1.Code.Gameplay.Weapons.Logic;

namespace Galactic1.Code.Gameplay.Units
{
    public interface IWeapon
    {
        WeaponState State { get; }
        int Durability { get; }
        float Durability01 { get; }
        int CurrentAmmo { get; }
        int ClipSize { get; }
        bool CanFire { get; }

        void Fire(FireContext context);
        void Reload();
    }
}