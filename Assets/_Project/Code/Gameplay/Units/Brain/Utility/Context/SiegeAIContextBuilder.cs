using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Siege-версия AIContextBuilder. Perception player-only (через
    /// TargetingUtility.FindNearestHostilePlayer) — здания в ctx.VisibleTarget
    /// никогда не попадают, поэтому AttackAction/PackChaseAction, переиспользуемые
    /// Siege-брейном, продолжают работать корректно с приоритетом "player highest".
    ///
    /// Порядок операций внутри Fill() критичен:
    ///   1. player perception
    ///   2. HQ resolution (кешируется один раз)
    ///   3. EnsureSubscribed на UnitMover.OnPathComputed
    ///   4. dead-wall guard — ДО формирования ctx.PathBlocked/ctx.BlockingWall
    ///   5. запись ctx из blackboard
    ///   6. ObjectiveResolver — получает уже очищенный ctx
    /// </summary>
    public static class SiegeAIContextBuilder
    {
        public static void Fill(
            UnitInstance unit,
            float dt,
            SiegeAIContext ctx,
            SiegeBlackboard blackboard,
            TargetingDefinition targeting,
            SiegePathService pathService,
            SiegeObjectiveResolver objectiveResolver)
        {
            ctx.DeltaTime = dt;
            ctx.CurrentState = unit.StateMachine.CurrentStateId;

            // ── 1. Player-only perception (НЕ FindNearestHostile!) ─────────
            var visiblePlayer = TargetingUtility.FindNearestHostilePlayer(unit);
            ctx.VisibleTarget = visiblePlayer;
            ctx.HasVisibleTarget = visiblePlayer != null;

            if (ctx.HasVisibleTarget)
            {
                ctx.VisibleTargetPosition = visiblePlayer.Position;
                ctx.DistanceToVisibleTarget = Vector3.Distance(unit.transform.position, visiblePlayer.Position);
                ctx.VisibleTargetHealthNormalized = GetNormalizedHp(visiblePlayer);

                blackboard.AggroTargetId = visiblePlayer.TargetId;
                blackboard.LastKnownTargetPosition = visiblePlayer.Position;
                blackboard.LastTimeSawTarget = Time.time;
                blackboard.AlertPhase = AlertPhase.Combat;
            }

            blackboard.TimeSinceSawTarget = blackboard.LastTimeSawTarget > 0f
                ? Time.time - blackboard.LastTimeSawTarget
                : float.MaxValue;

            ctx.HasAggroTarget = blackboard.HasAggroTarget;
            ctx.LastKnownTargetPosition = blackboard.LastKnownTargetPosition;
            ctx.TimeSinceSawTarget = blackboard.TimeSinceSawTarget;
            ctx.IsTargetInMemory = blackboard.HasAggroTarget
                                   && !ctx.HasVisibleTarget
                                   && ctx.TimeSinceSawTarget < targeting.LoseTargetDelay;

            if (blackboard.HasAggroTarget && !ctx.HasVisibleTarget
                && ctx.TimeSinceSawTarget >= targeting.LoseTargetDelay)
            {
                blackboard.ClearAggro();
                ctx.HasAggroTarget = false;
                ctx.IsTargetInMemory = false;
                if (blackboard.AlertPhase == AlertPhase.Combat)
                    blackboard.AlertPhase = AlertPhase.Calm;
            }

            if (blackboard.CommitTimeRemaining > 0f) blackboard.CommitTimeRemaining -= dt;
            if (blackboard.AttackCooldownRemaining > 0f) blackboard.AttackCooldownRemaining -= dt;

            // ── 2. HQ — резолвится и кешируется один раз ────────────────────
            blackboard.Headquarters ??= ResolveHeadquarters();
            ctx.Headquarters = blackboard.Headquarters;
            
            // ближайшая валидная AttackPoint HQ. Используется
            // SiegeAdvanceAction (движение) и SiegeAttackHQAction (range check)
            // вместо Headquarters.Position.
            ctx.HeadquartersAttackPosition = ctx.Headquarters != null
                ? SiegeAttackPointResolver.Resolve(unit, ctx.Headquarters, blackboard)
                : Vector3.zero;
            
#if UNITY_EDITOR
            // Debug.Log($"[Siege] unit={unit.name} " +
            //           $"attackPos={ctx.HeadquartersAttackPosition} " +
            //           $"unitPos={unit.transform.position} " +
            //           $"dist={Vector3.Distance(unit.transform.position, ctx.HeadquartersAttackPosition):F2} " +
            //           $"points={(ctx.Headquarters as TargetInfoBase)?.AttackPoints.Count}");
#endif
            

            // ── 3. Подписка на path-события (идемпотентно) ──────────────────
            pathService.EnsureSubscribed(unit, blackboard);

            // ── 4. Dead-wall guard — ДО ctx и ДО ObjectiveResolver ───────────
            // Независимый defensive-слой поверх event-based OnDestroyed:
            // даже если подписка на уничтожение не сработала (см. допущение
            // про IRaidFacilityRuntime cast в SiegePathService), мёртвая стена
            // никогда не попадёт в резолвер как валидная блокирующая цель.
            if (blackboard.CurrentWall != null && blackboard.CurrentWall.IsDead)
            {
                blackboard.CurrentWall = null;
                blackboard.PathBlocked = false;
            }

            ctx.PathBlocked = blackboard.PathBlocked;
            ctx.BlockingWall = blackboard.CurrentWall;
            ctx.HasReachablePath = !blackboard.PathBlocked;
            ctx.ObjectiveDistance = ctx.Headquarters != null
                ? Vector3.Distance(unit.transform.position, ctx.Headquarters.Position)
                : 0f;

            // ── 5. Приоритет цели ────────────────────────────────────────────
            blackboard.CurrentObjective = objectiveResolver.Resolve(unit, ctx, blackboard);
            ctx.CurrentObjective = blackboard.CurrentObjective;
        }

        /// <summary>ДОПУЩЕНИЕ: RaidCombatFacilityRuntime.Id совпадает с ключом,
        /// под которым сцен-инстанс HQ зарегистрирован в BaseFacilityRepository.
        /// Проверьте это соответствие в вашем проекте перед использованием.</summary>
        private static ITargetInfo ResolveHeadquarters()
        {
            var raid = ServiceLocator.Current.Get<GameSession>().GameLoopContext.CurrentRaid;
            var hqRuntime = raid?.DefenseFacilities?.GetFacility(FacilityType.CampHQ);
            if (hqRuntime == null) return null;

            var repo = ServiceLocator.Current.Get<BaseFacilityRepository>();
            var (found, instance) = repo.TryGet(hqRuntime.Id);
            return found ? instance.GetComponent<TargetInfoBase>() : null;
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
