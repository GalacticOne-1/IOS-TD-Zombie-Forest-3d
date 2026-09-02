using System;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    public class ObjectContext: MonoBehaviour, IObjectContext
    {
        [field: SerializeField] public Vector3 Size {get; private set;}



        public Vector3 PivotCenter() => transform.position;
            //=> new (transform.position.x + Size.x / 2, transform.position.y + Size.y / 2, transform.position.z + Size.z / 2);

        public Vector3 PivotCenterBottom() 
            => new (transform.position.x + Size.x / 2, transform.position.y, transform.position.z);


        private void OnDrawGizmos()
        {
            if (Size == Vector3.zero)
            {
                Size = transform.Find("SelectionCollider").GetComponent<BoxCollider>().size;
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(PivotCenter(), Size);
        }
    }
}