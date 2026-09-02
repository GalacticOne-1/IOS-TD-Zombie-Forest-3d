using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class EntityColliderObstacle : MonoBehaviour, IEntityColliderObstacle
    {
        public GameObject GameObject => gameObject;
    }

    public interface IEntityColliderObstacle
    {
        GameObject GameObject { get; }
    }
}