
namespace Galactic1.Code.Gameplay.BaseBuilding
{
    public interface ITurret
    {
        int Ammo { get; }
        int MaxAmmo { get; }
        float Range { get; }
    }
}