using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние движения.
    /// </summary>
    public class PlayerMoveState : PlayerState
    {
        //private JoystickController _controller;

        public PlayerMoveState(PlayerStateMachine machine) : base(machine)
        {
            //_controller = JoystickController.I;
        }

        public override void Enter()
        {
            // Включаем анимацию движения
            
            // если есть активный Job — отменить
            //JoystickController.I.Machine.ActionController.CancelCurrentJob();
        }

        public override void Update()
        {
            if (Machine.IsInputBlocked()) return;

            // Обработка движения: чтение осей, применение силы/velocity к Rigidbody2D
            // Если нет ввода движения — вернуть в Idle
            
            //ServiceLocator.Current.Get<HeroStateMachine>().Current.xMove = _controller.X;
            //ServiceLocator.Current.Get<HeroStateMachine>().Current.vMove = _controller.V;
            //ServiceLocator.Current.Get<HeroStateMachine>().Current.Movement(_controller.borderX, _controller.borderY);
        }

        public override void Exit()
        {
            // остановка движение анимация
            ServiceLocator.Current.Get<HeroStateMachine>().Current.EndMovement();
        }
    }
}