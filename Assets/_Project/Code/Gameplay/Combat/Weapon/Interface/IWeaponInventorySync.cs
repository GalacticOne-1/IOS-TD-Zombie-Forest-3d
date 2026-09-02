using Galactic1.Code.Gameplay.Weapons.Logic;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    /// <summary>
    /// Синхронизация Weapon → Inventory.
    /// Не даёт Weapon знать про конкретную реализацию инвентаря.
    /// </summary>
    public interface IWeaponInventorySync
    {
        void Bind(WeaponEntity entity);
        void Unbind(WeaponEntity entity);
    }
}