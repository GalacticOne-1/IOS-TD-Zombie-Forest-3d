
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public abstract class _Target
    {
        protected _Entity Entity;

        protected ITarget iTarget;

        public ITarget ITarget => iTarget;

        public _Target(_Entity entity)
        {
            this.Entity = entity;
        }


        protected Vector3 ray_start, ray_target;
        protected RaycastHit2D raycastHit2D;

        
        
        
        
        


        /// <summary>
        /// For add target
        /// </summary>
        /// <param name="target"></param>
        public void NewTarget(ITarget target)
        {
            iTarget = target;
            //DLog.Alert($"{Entity} : target status [{iTarget}]");
        }

        /// <summary>
        ///  For remove
        /// </summary>
        public void RemoveTarget()
        {
            iTarget = null;
            //DLog.Alert($"{Entity} : target status [{iTarget}]", EDlogColor.ORANGE);
        }



        
        /// <summary>
        /// ! Для общего стандарта !
        /// </summary>
        /// <param name="range"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool InRadius(float range, float distance) => distance <= range;

        /// <summary>
        /// true - цель в радиусе
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        //public bool InRadius(float range) => Vector3.Distance(unit.tr.position, iTarget.tr.position) < range;
        // не используем !!!
        // тк с коорд всегда проблемы из-за разности позиции с коллайдером
        
        public bool InRadius(float range)       // * используем этот вариант (дистанция через коллайдер)
        {
            UnderRay(range, out bool underRay, out bool inRadius);
            return underRay && inRadius;
        }

        /// <summary>
        /// пускаем луч в сторону цели
        /// </summary>
        /// <returns></returns>
        /// <param name="attackRange">for check radius</param>
        /// <param name="underRay"></param>
        /// <param name="inRadius"></param>
        public abstract void UnderRay(float attackRange, out bool underRay, out bool inRadius);
        /// <summary>
        /// пускаем луч в сторону цели
        /// </summary>
        /// <returns></returns>
        /// <param name="attackRange">for check radius</param>
        /// <param name="underRay"></param>
        /// <param name="inRadius"></param>
        public abstract void UnderRay(float attackRange, out bool underRay, out bool inRadius, out GameObject hitRay);


        /// <summary>
        /// true - проверка цели на активность
        /// </summary>
        /// <returns></returns>
        public bool IsLive() => iTarget != null && iTarget.IsLive;

        /// <summary>
        /// true - провекрка цели в радиусе и под лучом
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        //public bool AvailToAttack(float range) => InRadius(range) && UnderRay();
        public bool AvailToAttack(float range)
        {
            UnderRay(range, out bool underRay, out bool inRadius);
            return underRay && inRadius;
        }

        /// <summary>
        /// true - полная проверка цели
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        //public bool Available(float range) => IsLive() && InRadius(range) && UnderRay();
        public bool Available(float range)
        {
            UnderRay(range, out bool underRay, out bool inRadius);
            return IsLive() && underRay && inRadius;
        }




    }
}