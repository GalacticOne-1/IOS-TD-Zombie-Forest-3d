using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.PoolObject
{
    /// <summary>
    /// Контракт для любого poolable объекта.
    /// OnSpawn намеренно не принимает config — объект
    /// получает её один раз через IPoolItemConfig.SetConfig()
    /// при создании (или при смене конфига через Reconfigure).
    /// </summary>
    public interface IPoolable
    {
        void OnCreate();          // один раз при Instantiate
        void OnSpawn();           // при взятии из пула
        void OnDespawn();         // при возврате в пул
        void ResetState();        // сброс позиции, velocity, таймеров
        void SetPoolKey(RuntimeId poolKey);
        RuntimeId PoolKey { get; }
    }
    
}