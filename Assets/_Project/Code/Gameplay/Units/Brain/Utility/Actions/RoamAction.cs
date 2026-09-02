using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Роуминг — поведение по умолчанию.
    /// Score 1.0 без aggro/noise, 0.1 иначе (fallback).
    /// </summary>
    public sealed class RoamAction : IAIAction
    {
        public AIActionType Type => AIActionType.Roam;

        public ActionDecision Evaluate(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard)
        {
            bool hasAnything = ctx.HasVisibleTarget || ctx.IsTargetInMemory || ctx.HeardNoise;
            return new ActionDecision(hasAnything ? 0f : 0.15f);
        }

        public void Execute(UnitInstance unit, AIContext ctx, EnemyBlackboard blackboard, ActionDecision decision)
        {
            if (blackboard.AlertPhase == AlertPhase.Combat)
                blackboard.AlertPhase = AlertPhase.Calm;

            unit.StateMachine.Execute(new RoamCommand());
        }
    }
}