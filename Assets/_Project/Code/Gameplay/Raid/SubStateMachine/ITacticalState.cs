namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    /// <summary>
    /// Интерфейс состояния Tactical Sub-StateMachine.
    /// Все состояния рейда должны его реализовывать.
    /// </summary>
    public interface ITacticalState
    {
        void Enter(DIContainer container, GameLoopContext context);
        void Update(GameLoopContext context, float deltaTime);
        void Exit(GameLoopContext context);
    }
}