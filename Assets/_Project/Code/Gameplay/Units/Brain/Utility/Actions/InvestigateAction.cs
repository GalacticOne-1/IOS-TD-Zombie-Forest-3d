using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Исследование источника звука.
    ///
    /// Активируется когда:
    ///   — blackboard.HeardNoise == true
    ///   — Нет текущей visible или aggro цели (не прерываем chase)
    ///
    /// Score: 0.35 базовый + до 0.15 по интенсивности шума.
    /// Итого max 0.5 — ниже Chase (0.85) и Search (0.5 → 0.15), выше Roam (0.1).
    ///
    /// По прибытии: HeardNoise сбрасывается.
    /// </summary>
    public sealed class InvestigateAction : IAIAction
    {
        public AIActionType Type => AIActionType.Investigate;

        private readonly float _moveSpeed;
        private readonly float _arrivalRadius;

        public InvestigateAction(EnemyRuntimeDefinition def, float arrivalRadius = 2f)
        {
            _moveSpeed = def.MovementDefinition.WalkSpeed;
            _arrivalRadius = arrivalRadius;
        }

        public ActionDecision Evaluate(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard)
        {
            if (!ctx.HeardNoise) return ActionDecision.Zero;
            if (ctx.HasVisibleTarget) return ActionDecision.Zero;
            if (ctx.IsTargetInMemory) return ActionDecision.Zero;

            float score = 0.35f + ctx.NoiseIntensity * 0.15f;
            return new ActionDecision(Mathf.Clamp01(score), ctx.NoisePosition);
        }

        public void Execute(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard, ActionDecision decision)
        {
            blackboard.AlertPhase = AlertPhase.Suspicious;

            if (Vector3.Distance(unit.transform.position, ctx.NoisePosition) <= _arrivalRadius)
            {
                blackboard.ClearNoise();
                blackboard.AlertPhase = AlertPhase.Calm;
                return;
            }

            unit.StateMachine.Execute(new ChaseCommand("_noise", decision.MovePosition, _moveSpeed));
        }
    }
}