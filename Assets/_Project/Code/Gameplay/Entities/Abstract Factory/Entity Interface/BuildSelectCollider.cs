using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class BuildSelectCollider : MonoBehaviour, IEntitySelectCollider
    {
        public _Entity GetEntity() => transform.parent.GetComponent<_Entity>();
    }
}