using UnityEngine;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние смерти игрока — блокирует всё.
    /// </summary>
    public class PlayerDeathState : PlayerState
    {
        public PlayerDeathState(PlayerStateMachine machine) : base(machine) { }

        public override bool BlocksInput => true;

        public override void Enter()
        {
            // проиграть анимацию смерти, отключить контроллеры и т.д.
        }
    }
}