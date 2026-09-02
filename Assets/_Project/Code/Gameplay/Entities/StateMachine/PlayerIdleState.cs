using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Idle — базовое состояние ожидания игрока.
    /// </summary>
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            // Включаем контроллер перемещения/анимацию Idle
            // например: Animator.Play("Idle")
        }

        public override void Update()
        {
            // читаем ввод, если он доступен и переключаем в Move/Interact/Attack
            if (Machine.IsInputBlocked()) return;

            // простой пример: WASD / joystick обработка (псевдокод)
            // если движение -> переключиться в MoveState
            // если кнопка Action -> Machine.RequestInteract()
            // если кнопка Attack -> Machine.RequestAttack()
        }
    }
}