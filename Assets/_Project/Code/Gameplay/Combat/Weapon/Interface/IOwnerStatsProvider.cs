namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public interface IOwnerStatsProvider
    {
        float GetAccuracyModifier(); // 0..1
        float GetStressLevel(); // 0..100
        bool IsMoving();
    }
}