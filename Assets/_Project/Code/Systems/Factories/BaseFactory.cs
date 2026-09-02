using UnityEngine;

namespace Galactic1.Core.Systems.Factories
{
    /// <summary>
    /// Base class for all factories in the game.
    /// Provides generic instantiation logic and ensures consistent entity creation.
    /// This approach mirrors LDoE, where each entity type has its own factory but follows a shared pattern.
    /// </summary>
    public abstract class BaseFactory<T> : ScriptableObject where T : MonoBehaviour
    {
        public abstract T Create(Vector3 position, Quaternion rotation);
    }
}