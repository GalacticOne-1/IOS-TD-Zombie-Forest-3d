using UnityEngine;
using Galactic1.Gameplay.Interaction;

namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние атаки — блокирует ввод на критический период атаки.
    /// </summary>
    public class PlayerAttackState : PlayerState
    {
        private ActionJob _job;

        public PlayerAttackState(PlayerStateMachine machine) : base(machine) { }

        public override bool BlocksInput => true;

        public void SetAttackJob(ActionJob job)
        {
            _job = job;
        }

        public override void Enter()
        {
            if (_job == null)
            {
                Machine.ChangeState(Machine.GetIdleState());
                return;
            }
        }

        public override void Update()
        {
            if (_job == null)
            {
                Machine.ChangeState(Machine.GetIdleState());
                return;
            }

            if (_job.IsCompleted)
            {
                Machine.ActionController.FinishCurrentJob();
                Machine.ChangeState(Machine.GetIdleState());
            }

            if (_job.IsCancelled)
            {
                Machine.ActionController.CancelCurrentJob();
                Machine.ChangeState(Machine.GetIdleState());
            }
        }
    }
}