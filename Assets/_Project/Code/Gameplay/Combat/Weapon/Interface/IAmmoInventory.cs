
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public interface IAmmoInventory
    {
        int PeekAmmo(RuntimeId ammoId);
        int TakeAmmo(RuntimeId ammoId, int amount);
    }
}