using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Атака в ближнем бою.
    ///
    /// Изменения:
    ///   — Проверяет AttackCooldownRemaining из Blackboard.
    ///   — Hysteresis: если последний chosen state был Chase и commit не истёк —
    ///     score снижается чтобы не переключаться обратно в атаку мгновенно.
    /// </summary>
    public sealed class AttackAction : IAIAction
    {
        public AIActionType Type => AIActionType.Attack;

        private readonly EnemyCombatDefinition _combat;

        // Относительные modifiers — не абсолютные scores
        private const float LowHpThreshold = 0.3f;
        private const float LowHpBonus = 0.1f; // добить выгоднее
        private const float CommitPenalty = 0.3f; // hysteresis на границе attackRange

        public AttackAction(EnemyCombatDefinition combat) => _combat = combat;

        public ActionDecision Evaluate(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard)
        {
            Debug.Log(
                $"AttackAction target={ctx.VisibleTarget?.TargetId} " +
                $"dist={ctx.DistanceToVisibleTarget:F2} " +
                $"attackRange={_combat.AttackRange:F2}");
            
            // if (ctx is SiegeAIContext siegeCtx &&
            //     siegeCtx.CurrentObjective != SiegeObjective.Player)
            //     return ActionDecision.Zero;
            
            if (!ctx.HasVisibleTarget) 
                return ActionDecision.Zero;
            
            if (ctx.DistanceToVisibleTarget > _combat.AttackRange) 
                return ActionDecision.Zero;
            
            if (blackboard.AttackCooldownRemaining > 0f) 
                return ActionDecision.Zero;

            float score = 1.0f;

            if (ctx.VisibleTargetHealthNormalized < LowHpThreshold)
                score += LowHpBonus;

            if (blackboard.LastChosenState == UnitStateId.Chasing
                && blackboard.CommitTimeRemaining > 0f)
                score -= CommitPenalty;

            return new ActionDecision(Mathf.Clamp01(score));
        }

        public void Execute(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard, ActionDecision decision)
        {
            blackboard.LastChosenState = UnitStateId.MeleeEngaging;
            blackboard.CommitTimeRemaining = _combat.AttackCooldown;
            blackboard.AttackCooldownRemaining = _combat.AttackCooldown;

            unit.StateMachine.Execute(
                new AttackCommand(ctx.VisibleTarget.TargetId, UnitStateId.MeleeEngaging));
        }
    }
}