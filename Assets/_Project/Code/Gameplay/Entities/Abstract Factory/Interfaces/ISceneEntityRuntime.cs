namespace Galactic1.Code.AbstractFactory
{
    /// <summary>
    /// Универсальный runtime источник для Scene Entity.
    /// Любая игровая сущность должна реализовывать этот интерфейс.
    /// </summary>
    public interface ISceneEntityRuntime 
    {
        string Id { get; }
        void Dispose();
    }
}