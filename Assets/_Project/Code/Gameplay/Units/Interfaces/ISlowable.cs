namespace Galactic1.Code.Gameplay.Units.Interfaces
{
    /// <summary>
    /// Юнит, к которому можно применить замедление.
    /// Реализуется SurvivorInstance, ZombieInstance и любым другим
    /// юнитом с мувером.
    /// </summary>
    public interface ISlowable
    {
        void ApplySlow(object source, float speedMultiplier);
        void RemoveSlow(object source);
    }
}