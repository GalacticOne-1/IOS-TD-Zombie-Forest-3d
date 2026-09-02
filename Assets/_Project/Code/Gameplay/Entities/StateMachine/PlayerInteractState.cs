
namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Состояние интеракции: запускается когда игрок выполняет длительное действие (ActionJob).
    /// Это состояние блокирует ввод (BlocksInput = true).
    /// </summary>
    public class PlayerInteractState : PlayerState
    {
        private ActionJob _job;

        public PlayerInteractState(PlayerStateMachine machine) : base(machine) { }

        public override bool BlocksInput => false;

        public override void Enter()
        {
            // сюда должен попасть ActionJob из PlayerActionController
            _job = Machine.ActionController.CurrentJob;
            if (_job == null)
            {
                // нет активного джоба — возвращаемся в Idle
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

            // если джоб завершился — перейти в Idle
            if (_job.IsCompleted)
            {
                Machine.ActionController.FinishCurrentJob();
                Machine.ChangeState(Machine.GetIdleState());
            }

            // если джоб был прерван (например, игрок получил урон) — перейти в Idle
            if (_job.IsCancelled)
            {
                Machine.ActionController.CancelCurrentJob();
                Machine.ChangeState(Machine.GetIdleState());
            }
        }

        public override void Exit()
        {
            // очистка при выходе
            
        }
    }
}