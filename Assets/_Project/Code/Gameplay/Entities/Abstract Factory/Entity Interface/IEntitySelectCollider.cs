namespace Galactic1.AbstractFactory
{
    public interface IEntitySelectCollider
    {
        /// <summary>
        /// Доступ к компоненту из любого нижнего уровня внутри объекта
        /// </summary>
        /// <returns></returns>
        _Entity GetEntity();
    }
}