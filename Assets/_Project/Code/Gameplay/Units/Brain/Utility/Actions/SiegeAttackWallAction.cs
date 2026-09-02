using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Атака стены, которая подтверждённо блокирует путь к HQ.
    ///
    /// Wall attack разрешён только когда SiegeObjectiveResolver
    /// действительно выбрал Wall как текущую цель.
    ///
    /// Важно:
    /// наличие стены в AttackRange само по себе НЕ является достаточным
    /// условием для атаки. Решение об objective принимает
    /// SiegeObjectiveResolver.
    /// </summary>
    public sealed class SiegeAttackWallAction : IAIAction
    {
        public AIActionType Type => AIActionType.AttackWall;

        private readonly float _moveSpeed;
        private readonly float _attackRange;

        public SiegeAttackWallAction(float walkSpeed, float attackRange)
        {
            _moveSpeed = walkSpeed;
            _attackRange = attackRange;
        }

        public ActionDecision Evaluate(
            UnitInstance unit,
            AIContext context,
            EnemyBlackboard blackboard)
        {
            var ctx = (SiegeAIContext)context;
            var bb = (SiegeBlackboard)blackboard;

            // Objective является authoritative.
            if (bb.CurrentObjective != SiegeObjective.Wall)
                return ActionDecision.Zero;

            var wall = ctx.BlockingWall;

            if (wall == null || wall.IsDead)
                return ActionDecision.Zero;

            Vector3 attackPos =
                wall.GetClosestPoint(unit.transform.position);

            float distance =
                Vector3.Distance(unit.transform.position, attackPos);

            // Если стена уже не в радиусе атаки —
            // WallAction не должна инициировать MeleeEngaging.
            //
            // В этом случае она всё равно может быть валидной action
            // для Chase, но не должна "продавить" атаку.
            if (distance > _attackRange)
            {
                return new ActionDecision(
                    0.6f,
                    attackPos);
            }

            return new ActionDecision(
                0.6f,
                attackPos);
        }

        public void Execute(
            UnitInstance unit,
            AIContext context,
            EnemyBlackboard blackboard,
            ActionDecision decision)
        {
            var ctx = (SiegeAIContext)context;
            var bb = (SiegeBlackboard)blackboard;

            var wall = ctx.BlockingWall;

            if (wall == null || wall.IsDead)
                return;

            Vector3 attackPos =
                wall.GetClosestPoint(unit.transform.position);

            float distance =
                Vector3.Distance(unit.transform.position, attackPos);

            bb.CommitTimeRemaining = 0.3f;

            if (distance <= _attackRange)
            {
                bb.LastChosenState = UnitStateId.MeleeEngaging;

                unit.StateMachine.Execute(
                    new AttackCommand(
                        wall.TargetId,
                        UnitStateId.MeleeEngaging));
            }
            else
            {
                bb.LastChosenState = UnitStateId.Chasing;

                unit.StateMachine.Execute(
                    new ChaseCommand(
                        wall.TargetId,
                        attackPos,
                        _moveSpeed));
            }
        }
    }
}