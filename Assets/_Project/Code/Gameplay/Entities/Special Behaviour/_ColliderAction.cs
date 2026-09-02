using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _ColliderAction : MonoBehaviour, IColliderAction
    {
        protected _Entity Entity;


        public virtual void Initialize(_Entity entity)
        {
            Entity = entity;
        }

        

        /// <summary>
        /// Some entity trigger enter
        /// </summary>
        /// <param name="obj">collider object</param>
        public abstract void ColliderEnter(GameObject obj);
        
        /// <summary>
        /// Some entity trigger exit
        /// </summary>
        /// <param name="obj">collider object</param>
        public abstract void ColliderExit(GameObject obj);
    }

    public interface IColliderAction
    {
        void Initialize(_Entity entity);
        
        /// <summary>
        /// Some entity trigger enter
        /// </summary>
        /// <param name="obj">collider object</param>
        void ColliderEnter(GameObject obj);
        
        /// <summary>
        /// Some entity trigger exit
        /// </summary>
        /// <param name="obj">collider object</param>
        void ColliderExit(GameObject obj);
    }
}