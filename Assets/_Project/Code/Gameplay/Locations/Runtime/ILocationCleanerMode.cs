namespace Galactic1.Gameplay.Locations
{
    /// <summary>
    /// Интерфейс для очистки локации (undo load).
    /// </summary>
    public interface ILocationCleanerMode
    {
        void Clear(LocationContext ctx);
    }
}