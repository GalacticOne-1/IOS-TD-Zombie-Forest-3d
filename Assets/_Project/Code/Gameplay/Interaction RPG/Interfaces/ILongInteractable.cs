namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Опциональный интерфейс: если интеракт требует времени, объект может реализовать этот интерфейс.
    /// </summary>
    public interface ILongInteractable
    {
        float RequiredTime { get; }
        bool RequiresProgressBar { get;}
        
        /// true - объект в финальном состоянии (сундук открыт / дерево срублено ...) 
        bool IsFinished { get; }
    }
}