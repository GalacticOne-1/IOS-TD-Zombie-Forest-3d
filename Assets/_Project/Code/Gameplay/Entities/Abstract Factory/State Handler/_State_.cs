

namespace Galactic1.AbstractFactory
{
    public abstract class _State_
    {
        protected _Entity Entity;
        protected FSM FSM;


        #region TIMER TRANSITION

        protected CStateTransition transition;

        protected bool requestedExit;

        protected _Timer timer;
        protected bool onExit;

        #endregion

        
        
        

        public _State_(_Entity entity, FSM fsm, CStateTransition setup)
        {
            this.Entity = entity;
            FSM = fsm;
            transition = setup;
        }


        
        
        /// <summary>
        /// Реализация входа в состояние
        /// </summary>
        public virtual void Enter()
        {
#if UNITY_EDITOR
            Entity.Log(new CEntityDebugParam()
            {
                Message = "State Movement : Enter"
            });
#endif
            onExit = false;
            requestedExit = false;
            timer = new _Timer();
        }

        /// <summary>
        /// For update
        /// </summary>
        public virtual void Logic()
        {
            // если запрос на выход из состояния и нужно дождатся таймера
            if (requestedExit && timer.Elapsed >= transition.exitTime)
            {
#if UNITY_EDITOR
                DLog.Alert($"State {this} : Transition complete {timer.Elapsed}");
#endif
                onExit = true;
                Exit();
                FSM.TransitionComplete();
            }
        }

        /// <summary>
        /// Реализация выхода
        /// </summary>
        protected abstract void Exit();
        
        public abstract bool CanExit();

        /// <summary>
        /// Запрос для выхода из состояния
        /// </summary>
        public void ExitRequest()
        {
            Entity.Log(new CEntityDebugParam() { Message = $"Request exit from {this}" });
            if (!transition.needExitTime)
            {
                onExit = true;
                Exit();
                FSM.TransitionComplete();
            }
            else
            {
                requestedExit = true;
            }
        }

        
        /// <summary>
        /// Запрос для выхода из состояния при смерти
        /// <br/>(игнорирует блокировку от процесса)
        /// </summary>
        public virtual void ExitForDeath()
        {
            FSM.TransitionComplete();
        }
    }
}