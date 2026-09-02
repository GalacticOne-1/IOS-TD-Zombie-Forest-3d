
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class ColliderDetect : MonoBehaviour
    {
        /*
         *      Обнаружение других koллайдеров
         */

        //public _Entity Entity { get; private set; }
        private IColliderAction ColliderAction;
        

        public void Initialize(_Entity entity)
        {
            //this.Entity = entity;
            ColliderAction = entity.GetComponent<IColliderAction>();
        }
        
        private void OnTriggerEnter2D(Collider2D col) => ColliderAction.ColliderEnter(col.gameObject);

        private void OnTriggerExit2D(Collider2D col) => ColliderAction.ColliderExit(col.gameObject);
    }
}