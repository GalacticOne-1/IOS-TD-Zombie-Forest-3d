using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние движения.
    /// </summary>
    public class PlayerJumpState : PlayerState
    {
        //private JoystickController _controller;

        public PlayerJumpState(PlayerStateMachine machine) : base(machine)
        {
            //_controller = JoystickController.I;
        }

        public override void Enter()
        {
            // если есть активный Job — отменить
            //JoystickController.I.Machine.ActionController.CancelCurrentJob();
            
            ServiceLocator.Current.Get<HeroStateMachine>().Current.Jumping();
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