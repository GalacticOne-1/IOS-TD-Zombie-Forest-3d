using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    public sealed class PackChaseAction : IAIAction
    {
        public AIActionType Type => AIActionType.Chase;

        private readonly TargetingDefinition _targeting;
        private readonly EnemyCombatDefinition _combat;
        private readonly float _chaseSpeed;
        private readonly bool _usePack; // из definition.BrainDefinition.UsePackBehaviour

        private const float BaseScore = 0.7f;
        private const float PackBonus = 0.15f; // применяется только если _usePack
        private const float LowHpBonus = 0.1f;
        private const float LowHpThreshold = 0.3f;
        private const float AlertBonus = 0.05f;

        public PackChaseAction(EnemyRuntimeDefinition def)
        {
            _targeting = def.TargetingDefinition;
            _combat = def.CombatDefinition;
            _chaseSpeed = def.MovementDefinition.RunSpeed;
            _usePack = def.BrainDefinition.UsePackBehaviour;
        }

        public ActionDecision Evaluate(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard)
        {
            // if (ctx is SiegeAIContext siegeCtx &&
            //     siegeCtx.CurrentObjective != SiegeObjective.Player)
            //     return ActionDecision.Zero;
            
            if (!ctx.HasVisibleTarget && !ctx.HasAggroTarget) 
                return ActionDecision.Zero;

            Vector3 chaseTarget = ctx.HasVisibleTarget
                ? ctx.VisibleTargetPosition
                : ctx.LastKnownTargetPosition;

            float dist = Vector3.Distance(unit.transform.position, chaseTarget);
            // Debug.Log(
            //     $"ChaseAction dist={dist:F2} " +
            //     $"attackRange={_combat.AttackRange:F2}");
            
            if (dist > _targeting.LoseTargetRange) return ActionDecision.Zero;
            if (dist <= _combat.AttackRange) return ActionDecision.Zero;

            Vector3 slotPos = blackboard.PackReservation.PeekSlotPosition(
                blackboard.AggroTargetId ?? "_memory", chaseTarget, unit);

            float score = BaseScore;

            // PackBonus применяется только если archetype использует pack behaviour
            if (_usePack)
                score += PackBonus;

            if (ctx.HasVisibleTarget && ctx.VisibleTargetHealthNormalized < LowHpThreshold)
                score += LowHpBonus;

            if (ctx.AlertPhase == AlertPhase.Combat)
                score += AlertBonus;

            return new ActionDecision(Mathf.Clamp01(score), slotPos);
        }

        public void Execute(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard, ActionDecision decision)
        {
            string targetId = blackboard.AggroTargetId ?? "_memory";
            Vector3 chaseTarget = ctx.HasVisibleTarget
                ? ctx.VisibleTargetPosition
                : ctx.LastKnownTargetPosition;

            // БЫЛО: результат EnsureSlot выбрасывался, в команду шёл decision.MovePosition
            // СТАЛО: используем фактически зарезервированную позицию слота
            Vector3 slotPosition = blackboard.PackReservation.EnsureSlot(
                targetId, chaseTarget, unit, blackboard);

            blackboard.LastChosenState = UnitStateId.Chasing;
            blackboard.CommitTimeRemaining = 0.3f;

            unit.StateMachine.Execute(new ChaseCommand(targetId, slotPosition, _chaseSpeed));
        }
    }
}