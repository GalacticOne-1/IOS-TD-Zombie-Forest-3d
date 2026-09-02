using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class AttackContainer : MonoBehaviour
    {
        public _Entity Entity { get; private set; }
        
        public bool fullAttack { get; private set; }

        [SerializeField] private GameObject pistolSocket, rifleSocket;

        [field: SerializeField] public GameObject containerMainWeapon {get; private set;}
        
        
        private _Attack mainWeapon;
        private _Attack subWeapon;

        
        
        
        


        public _Attack MainWeapon => mainWeapon;

        public _Attack SubWeapon => subWeapon;



        
        
        
        
        


        /*
         *      Вызываем при каждой активации юнита
         *      (сбрасываем оружие в начальное состояние, готовое к использованию)
         */

        /// <summary>
        /// Активация 
        /// </summary>
        /// <param name="entity"></param>
        public void Activate(_Entity entity)
        {
            this.Entity = entity;
            
            // *** сбрасываем состояние оружия 
            if (mainWeapon) mainWeapon.Initialize(this);
            if (subWeapon) subWeapon.Initialize(this);
        }
        
        public void SetupFullAttack(bool full) => fullAttack = full;

        public void AddMainWeapon(_Attack wp) => mainWeapon = wp;
        public void RemoveMainWeapon()
        {
            if(mainWeapon)
            {
                Destroy(mainWeapon.gameObject);
                mainWeapon = null;
            }
        }
        
        
        
        public void AddSubWeapon(_Attack wp) => subWeapon = wp;

        public void RemoveSubWeapon()
        {
            if (subWeapon)
            {
                Destroy(subWeapon.gameObject);
                subWeapon = null;
            }
        }
        





        #region LOGIC

        public void Begin()
        {
            // * перенести в _AI.MovementComplete() перед атакой ???
            Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Idle);
            if (mainWeapon) mainWeapon.ResetState();
        }

        /// <summary>
        /// Процесс атаки
        /// </summary>
        public void Process()
        {
            if (mainWeapon) mainWeapon.Logic();
        }

        /// <summary>
        /// Постоянный процесс, не зависящий от текущего состояния
        /// </summary>
        public void HiddenProcess()
        {
            if (mainWeapon) mainWeapon.Reloading();
        }

        public bool CanStop() => mainWeapon.CanStop();

        public void Stop()
        {
            
        }


        /// <summary>
        /// Для остановки процесса сразу
        /// </summary>
        public void ForceStop()
        {
            if (mainWeapon) mainWeapon.ForceStop();
        }
        

        #endregion
        
        
        
        // private void OnDrawGizmos()
        // {
        //     if(mainWeapon)
        //     {
        //         Gizmos.color = Color.blue;
        //         Gizmos.DrawWireSphere(transform.position, mainWeapon.WeaponSetup.rangeDetect);
        //
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawWireSphere(transform.position, mainWeapon.WeaponSetup.rangeAttack);
        //     }
        // }
        
    }
}