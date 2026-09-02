using Galactic1.Core.Enums;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Optional weapon animator switching feature.
    /// </summary>
    public interface IWeaponAnimatorSwitcher
    {
        void SetWeapon(WeaponType type);
    }
}