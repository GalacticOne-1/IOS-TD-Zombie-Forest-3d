using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Дефолтное поведение Siege — движение к HQ. Аналог RoamAction для Raid.
    ///
    /// ЕДИНСТВЕННЫЙ класс во всей Siege-реализации, инициирующий движение
    /// к Headquarters. SiegePathService только наблюдает за результатом
    /// этого движения — не дублирует и не конкурирует с ним.
    /// </summary>
    public sealed class SiegeAdvanceAction : IAIAction
    {
        public AIActionType Type => AIActionType.AdvanceToHQ;

        private readonly float _moveSpeed;

        public SiegeAdvanceAction(float walkSpeed) => _moveSpeed = walkSpeed;

        public ActionDecision Evaluate(UnitInstance unit, AIContext context, EnemyBlackboard blackboard)
        {
            var ctx = (SiegeAIContext)context;
            var bb = (SiegeBlackboard)blackboard;

            if (bb.CurrentObjective != SiegeObjective.Headquarters) return ActionDecision.Zero;
            if (ctx.Headquarters == null) return ActionDecision.Zero;

            // Базовый score — "This priority is absolute" по ТЗ.
            // SiegeAttackHQAction перебивает его (0.9) только когда HQ в радиусе атаки.
            return new ActionDecision(0.2f, ctx.HeadquartersAttackPosition);
        }

        public void Execute(UnitInstance unit, AIContext context, EnemyBlackboard blackboard, ActionDecision decision)
        {
            var bb = (SiegeBlackboard)blackboard;
            bb.LastChosenState = UnitStateId.Chasing;
            bb.CommitTimeRemaining = 0.3f;

            unit.StateMachine.Execute(new ChaseCommand("_hq", decision.MovePosition, _moveSpeed));
        }
    }
}
