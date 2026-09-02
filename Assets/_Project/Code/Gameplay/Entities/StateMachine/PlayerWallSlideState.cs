using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние движения.
    /// </summary>
    public class PlayerWallSlideState : PlayerState
    {
        //private JoystickController _controller;

        public PlayerWallSlideState(PlayerStateMachine machine) : base(machine)
        {
            //_controller = JoystickController.I;
        }

        public override void Enter()
        {
            
        }

        public override void Update()
        {
            if (Machine.IsInputBlocked()) return;

            // Обработка движения: чтение осей, применение силы/velocity к Rigidbody2D
            // Если нет ввода движения — вернуть в Idle
        }

        public override void Exit()
        {
            // остановка движение анимация
        }
    }
}