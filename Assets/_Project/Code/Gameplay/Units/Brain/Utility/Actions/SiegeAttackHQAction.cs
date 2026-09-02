using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Атака HQ, когда он в радиусе атаки. Использует существующий combat
    /// pipeline (AttackCommand + ZombieMeleeEngagingState) — HQ не требует
    /// отдельной боевой логики, только ITargetInfo, который у него уже есть
    /// через FacilityTargetInfo : TargetInfoBase.
    /// </summary>
    public sealed class SiegeAttackHQAction : IAIAction
    {
        public AIActionType Type => AIActionType.AttackHQ;

        private readonly float _attackRange;

        public SiegeAttackHQAction(float attackRange) => _attackRange = attackRange;

        public ActionDecision Evaluate(UnitInstance unit, AIContext context, EnemyBlackboard blackboard)
        {
            var ctx = (SiegeAIContext)context;
            var bb = (SiegeBlackboard)blackboard;

            if (bb.CurrentObjective != SiegeObjective.Headquarters) return ActionDecision.Zero;
            if (ctx.Headquarters == null || ctx.Headquarters.IsDead) return ActionDecision.Zero;

            // CHANGED: было ctx.ObjectiveDistance (расстояние до центра HQ)
            float distToAttackPos = Vector3.Distance(unit.transform.position, ctx.HeadquartersAttackPosition);
            if (distToAttackPos > _attackRange) return ActionDecision.Zero;

            return new ActionDecision(0.9f, ctx.HeadquartersAttackPosition);
        }

        public void Execute(UnitInstance unit, AIContext context, EnemyBlackboard blackboard, ActionDecision decision)
        {
            var ctx = (SiegeAIContext)context;
            var bb = (SiegeBlackboard)blackboard;

            bb.LastChosenState = UnitStateId.MeleeEngaging;
            bb.CommitTimeRemaining = 0.3f;

            // Не изменено: TargetId остаётся HQ, не attack point.
            unit.StateMachine.Execute(new AttackCommand(ctx.Headquarters.TargetId, UnitStateId.MeleeEngaging));
        }
    }
}
