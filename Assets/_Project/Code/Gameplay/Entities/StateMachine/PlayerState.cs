using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Базовый класс состояния игрока.
    /// Все состояния наследуют этот класс и реализуют Enter/Exit/Update.
    /// </summary>
    public abstract class PlayerState
    {
        protected PlayerStateMachine Machine;
        protected GameObject Player => Machine.PlayerGameObject;
        protected Transform PlayerTransform => Player.transform;

        public PlayerState(PlayerStateMachine machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }

        /// <summary>Флаг блокировки ввода в этом состоянии (например, Interact/Attack)</summary>
        public virtual bool BlocksInput => false;
    }
}