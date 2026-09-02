using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Заполняет AIContext один раз за think-тик.
    ///
    /// Team filtering делегирован в TargetingUtility.FindNearestHostile() —
    /// единственная точка enemy-фильтрации в проекте.
    /// </summary>
    public static class AIContextBuilder
    {
        public static void Fill(
            UnitInstance unit,
            float dt,
            AIContext ctx,
            EnemyBlackboard blackboard,
            TargetingDefinition targeting)
        {
            ctx.DeltaTime = dt;
            ctx.CurrentState = unit.StateMachine.CurrentStateId;

            // ── 1. Ближайшая враждебная цель — через TargetingUtility ──────
            var visibleTarget = TargetingUtility.FindNearestHostile(unit);

            ctx.VisibleTarget = visibleTarget;
            ctx.HasVisibleTarget = visibleTarget != null;

            if (ctx.HasVisibleTarget)
            {
                ctx.VisibleTargetPosition = visibleTarget.Position;
                ctx.DistanceToVisibleTarget = Vector3.Distance(
                    unit.transform.position, visibleTarget.Position);
                ctx.VisibleTargetHealthNormalized = GetNormalizedHp(visibleTarget);

                // ── 2. Обновить aggro memory ──────────────────────────────
                blackboard.AggroTargetId = visibleTarget.TargetId;
                blackboard.LastKnownTargetPosition = visibleTarget.Position;
                blackboard.LastTimeSawTarget = Time.time;
                blackboard.AlertPhase = AlertPhase.Combat;
            }

            // ── 3. Aggro memory → Context ─────────────────────────────────
            blackboard.TimeSinceSawTarget = blackboard.LastTimeSawTarget > 0f
                ? Time.time - blackboard.LastTimeSawTarget
                : float.MaxValue;

            ctx.HasAggroTarget = blackboard.HasAggroTarget;
            ctx.LastKnownTargetPosition = blackboard.LastKnownTargetPosition;
            ctx.TimeSinceSawTarget = blackboard.TimeSinceSawTarget;
            ctx.IsTargetInMemory = blackboard.HasAggroTarget
                                   && !ctx.HasVisibleTarget
                                   && ctx.TimeSinceSawTarget < targeting.LoseTargetDelay;

            // Память истекла — сбросить aggro
            if (blackboard.HasAggroTarget
                && !ctx.HasVisibleTarget
                && ctx.TimeSinceSawTarget >= targeting.LoseTargetDelay)
            {
                blackboard.ClearAggro();
                ctx.HasAggroTarget = false;
                ctx.IsTargetInMemory = false;
                if (blackboard.AlertPhase == AlertPhase.Combat)
                    blackboard.AlertPhase = AlertPhase.Calm;
            }

            // ── 4. Noise → Context ────────────────────────────────────────
            ctx.HeardNoise = blackboard.HeardNoise;
            ctx.NoisePosition = blackboard.NoisePosition;
            ctx.NoiseIntensity = blackboard.NoiseIntensity;

            // ── 5. Alert phase → Context ──────────────────────────────────
            ctx.AlertPhase = blackboard.AlertPhase;

            // ── 6. Hysteresis tick ────────────────────────────────────────
            if (blackboard.CommitTimeRemaining > 0f)
                blackboard.CommitTimeRemaining -= dt;

            // ── 7. Attack cooldown tick ───────────────────────────────────
            if (blackboard.AttackCooldownRemaining > 0f)
                blackboard.AttackCooldownRemaining -= dt;
        }

        private static float GetNormalizedHp(ITargetInfo target)
        {
            var stats = target.Unit?.Stats;
            if (stats == null) return 1f;

            var hp = stats.Get(StatId.Health);
            float max = stats.MaxHP;
            return max > 0f ? Mathf.Clamp01(hp.Value / max) : 1f;
        }
    }
}