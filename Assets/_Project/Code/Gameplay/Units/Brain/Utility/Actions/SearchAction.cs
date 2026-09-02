using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Поиск в последней известной позиции цели.
    ///
    /// Активируется когда:
    ///   — Нет видимой цели
    ///   — Но AggroTargetId != null (память не истекла)
    ///   — TimeSinceSawTarget < LoseTargetDelay
    ///
    /// Score между Chase (0.85) и Roam (0.1):
    ///   0.5 — базовый поиск
    ///   Снижается линейно по мере истечения LoseTargetDelay.
    /// </summary>
    public sealed class SearchAction : IAIAction
    {
        public AIActionType Type => AIActionType.Search;

        private readonly float _moveSpeed;
        private readonly float _loseTargetDelay;

        public SearchAction(EnemyRuntimeDefinition def)
        {
            _moveSpeed = def.MovementDefinition.WalkSpeed;
            _loseTargetDelay = def.TargetingDefinition.LoseTargetDelay;
        }

        public ActionDecision Evaluate(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard)
        {
            if (!ctx.IsTargetInMemory) return ActionDecision.Zero;
            if (ctx.HasVisibleTarget) return ActionDecision.Zero;

            float progress = Mathf.Clamp01(ctx.TimeSinceSawTarget / _loseTargetDelay);
            float score = Mathf.Lerp(0.5f, 0.15f, progress);

            return new ActionDecision(score, ctx.LastKnownTargetPosition);
        }

        public void Execute(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard, ActionDecision decision)
        {
            blackboard.AlertPhase = AlertPhase.Alerted;
            blackboard.LastChosenState = UnitStateId.Chasing;

            unit.StateMachine.Execute(new ChaseCommand("_search", decision.MovePosition, _moveSpeed));
        }
    }
}