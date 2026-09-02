using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class FSM
    {
        private _Entity _entity;
        
        private Dictionary<EUnitStateType, _State_> list;

        private _State_ handler;
        
        
        private DFunc onExitRequest;

        public void OnExitRequest() => onExitRequest?.Invoke();

        public FSM(_Entity entity)
        {
            this._entity = entity;
            list = new Dictionary<EUnitStateType, _State_>();
        }
        
        
        
        
        
        


        /// <summary>
        /// Добавление состояния чтобы юнит мог использовать
        /// </summary>
        /// <param name="type">key</param>
        /// <param name="handler"></param>
        public void AddState(EUnitStateType type, _State_ handler)
        {
            list.Add(type, handler);
        }
        
        /// <summary>
        /// Для получения доступа к обработчику
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public _State_ GetStateHandler(EUnitStateType type) => list[type];







        #region LOGIC

        /// <summary>
        /// Вызывать при старте юнита
        /// </summary>
        /// <param name="newState"></param>
        public void Initialize(EUnitStateType newState)
        {
            handler = list[newState];
            handler.Enter();
        }

        /// <summary>
        /// Вызывать для смены состояния
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(EUnitStateType newState)
        {
            if (newState == EUnitStateType.DIE)
            {
                // *** выход при смерти без условий
                handler.ExitForDeath();  
                return;
            }
            
            
            if (!handler.CanExit())
            {
                // *** если невозможно запустить выход сразу 
                onExitRequest = handler.ExitRequest;
                return;
            }
            
            // *** выход из текущего состояния
            handler.ExitRequest();          
        }

        /// <summary>
        /// Вызывается из состояния, когда возможен переход
        /// </summary>
        public void TransitionComplete()
        {
            onExitRequest = null;
            
            // *** может перенести в событие handler.Enter() ??
            // т.к вход из за таймера может быть не сразу
            _entity.OnStateTransitionComplete();

            // if (!list.ContainsKey(unit.STATE))
            // {
            //     DLog.Alert($"FSM : not have state {unit.STATE} [unit {unit.name}]");
            // }
            handler = list[_entity.STATE];     // вход в новое состояние
            handler.Enter();
        }
        
        /// <summary>
        /// Обновление для текущего состояния
        /// </summary>
        public void Logic()
        {
            handler.Logic();
        }
        

        #endregion
        
        
        
        
        
    }
    
    public enum EUnitStateType
    {
        IDLE, MOVEMENT, CHASE, ATTACK, DIE
    }

    [System.Serializable]
    public struct CStateTransition
    {
        public bool needEnterTime;
        public bool needExitTime;
        public float enterTime;
        public float exitTime;
    }
}