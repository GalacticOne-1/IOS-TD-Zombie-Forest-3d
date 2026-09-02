
using System.Collections.Generic;
using Galactic1.Code.Utility;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1
{
    public class MonoBehaviourMaster : MonoBehaviour, IGameService
    {
        
        public List<IUpdate> update = new List<IUpdate>();
        public List<IUpdateSec> update_sec = new List<IUpdateSec>();
        public List<IFixedUpdate> fixedUpdate = new List<IFixedUpdate>();
        public List<ILateUpdate> lateUpdate = new List<ILateUpdate>();


        public event DFunc onUpdate;

        
        
        

        public bool isPause,        // пауза в игре 
            freeze;                 // полная остановка

        private float timer;
        public int sessionTime;
        public bool stopUpdate { set; get; }


        
        
        public void Activate()
        {
            //update.Add(EscController.I);
            //update.Add(DoubleClick.I);
        }
        

        private void Update()
        {
            if (!_GameState.AppLoaded) return;
            
            // обычное время в игре
            timer += Time.deltaTime;
            if (!stopUpdate && timer >= 1)
            {
                for (int i = 0; i < update_sec.Count; i++)
                {
                    update_sec[i].UpdateS();
                }
            }
            if (timer >= 1)
            {
                timer = 0;
                sessionTime++;
                TimeManagement.currDateInSeconds++;
            }
            
            
            if(freeze) return;
            

            if(isPause) return;
            
            PointerTracker.Update();
            GestureSystem.Update();

            // if (Input.anyKeyDown || Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
            // {
            //     EventBus<AnyKeyDownEvent>.Raise(new AnyKeyDownEvent());
            // }
            
            onUpdate?.Invoke();

            for (int i = 0; i < update.Count; i++)
            {
                update[i].UpdateM();
            }
        }

        private void FixedUpdate()
        {
            if(!_GameState.AppLoaded || freeze || isPause) return;
            
            for (int i = 0; i < fixedUpdate.Count; i++)
            {
                fixedUpdate[i].FixedUpdateM();
            }
        }
        
        /*private void LateUpdate()
        {
        if (!ApplicationSetup.APP_LOAD) return;
            if(isPause || saving) return;
            
            for (int i = 0; i < lateUpdate.Count; i++)
            {
                lateUpdate[i].LateUpdateM();
            }
        }*/
        
    }

    public interface IUpdate
    {
        /// <summary>
        /// Перед сменой сцен, нужно отписыватся от MonoBehaviourMaster
        /// </summary>
        void IUpdateClear();
        void UpdateM();
        
    }
    
    public interface IUpdateSec
    {
        /// <summary>
        /// Перед сменой сцен, нужно отписыватся от MonoBehaviourMaster
        /// </summary>
        void IUpdateClear();
        void UpdateS();
        
    }
    public interface IFixedUpdate
    {
        void FixedUpdateM();
        /// <summary>
        /// Перед сменой сцен, нужно отписыватся от updateM
        /// </summary>
        void IUpdateClear();
    }
    public interface ILateUpdate
    {
        void LateUpdateM();
        /// <summary>
        /// Перед сменой сцен, нужно отписыватся от updateM
        /// </summary>
        void IUpdateClear();
    }
}