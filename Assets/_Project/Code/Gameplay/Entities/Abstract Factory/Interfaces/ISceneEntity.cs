using UnityEngine;

namespace Galactic1.AbstractFactory
{
    /// <summary>
    /// Canonical scene-level contract.
    /// Represents any active entity in the world (units, buildings, vehicles, traps, etc.)
    ///
    /// НЕ содержит gameplay-логики.
    /// Используется как ownership reference layer.
    /// </summary>
    public interface ISceneEntity
    {
        string UniqueId { get; }
        Transform Tr { get; }
        GameObject GameObject { get; }
        bool TryGetCapability<T>(out T component) where T : class;
        void Entity_Destroy();
    }
}