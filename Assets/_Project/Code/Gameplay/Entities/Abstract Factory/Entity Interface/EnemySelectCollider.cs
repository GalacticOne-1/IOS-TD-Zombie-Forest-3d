using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class EnemySelectCollider : MonoBehaviour, IEntitySelectCollider
    {
        public _Entity GetEntity() => transform.parent.GetComponent<_Entity>();
    }
}