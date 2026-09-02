namespace Galactic1.Code.Systems.GameLoop
{
    /// <summary>
    /// Интерфейс состояния кор-лупа.
    /// Логика внутри состояния, UI подписывается на события.
    /// </summary>
    public interface IGameLoopState
    {
        GameLoopState Id { get; }
        void Enter(GameLoopContext context);
        void Exit(GameLoopContext context);
    }
}