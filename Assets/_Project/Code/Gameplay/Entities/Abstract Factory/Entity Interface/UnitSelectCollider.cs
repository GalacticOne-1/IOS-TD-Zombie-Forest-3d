using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class UnitSelectCollider : MonoBehaviour, IEntitySelectCollider
    {
        public _Entity GetEntity() => transform.parent.parent.GetComponent<_Entity>();
    }
}