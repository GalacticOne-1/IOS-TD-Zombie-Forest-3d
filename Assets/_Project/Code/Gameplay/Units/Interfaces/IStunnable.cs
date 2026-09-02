namespace Galactic1.Code.Gameplay.Units.Interfaces
{
    /// <summary>
    /// Юнит, к которому можно применить стан.
    /// Реализуется SurvivorInstance и любым юнитом со StateMachine,
    /// поддерживающим StunCommand.
    /// </summary>
    public interface IStunnable
    {
        void ApplyStun(float duration);
    }
}