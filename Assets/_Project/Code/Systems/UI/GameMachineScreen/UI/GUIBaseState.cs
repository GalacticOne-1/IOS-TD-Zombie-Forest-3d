using UnityEngine;

namespace Galactic1
{
    public abstract class GUIBaseState
    {
        public virtual void Enter()
        {
            
        }

        public virtual void HandleInput()
        {

        }

        public virtual void LogicUpdate()
        {

        }

        public virtual void PhysicsUpdate()
        {

        }

        public virtual void Exit()
        {

        }
    }

    public interface IStateMachineGUI
    {
        GUIBaseState currentState { set; get; }
        /// <summary>
        /// Инициализация стартовым состоянием
        /// </summary>
        /// <param name="state"></param>
        void Initialize(GUIBaseState state);
        
        /// <summary>
        /// Для изменения состояния
        /// </summary>
        /// <param name="newState"></param>
        public void SelectState(GUIBaseState newState);
    }
}