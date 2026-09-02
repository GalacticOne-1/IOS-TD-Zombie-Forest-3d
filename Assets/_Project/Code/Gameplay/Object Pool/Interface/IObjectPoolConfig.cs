using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Structure;

namespace Galactic1.PoolObject
{
    public interface IObjectPoolConfig
    {
        RuntimeId Id { get; }
        string PrefabPath { get; }
        ObjectPoolParam ObjectPoolParam { get; }
    }
}