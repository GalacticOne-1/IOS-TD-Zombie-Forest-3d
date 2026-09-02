using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// THE canonical targeting layer for all gameplay systems.
    ///
    /// ИЗМЕНЕНИЯ ДЛЯ SIEGE (аддитивные — существующие методы не менялись):
    ///   — FindNearestHostilePlayer(): аналог FindNearestHostile, но исключает
    ///     здания (HQ, стены). Используется SiegeAIContextBuilder, чтобы
    ///     ctx.VisibleTarget в Siege-контексте никогда не был зданием —
    ///     AttackAction/PackChaseAction, переиспользуемые Siege-брейном,
    ///     благодаря этому продолжают работать корректно.
    ///   — FindHostileById(): добавлен fallback на TargetInfoRegistry.TryGetById,
    ///     чтобы находить здания, не всегда попадающие в
    ///     PhysicsPerception.GetVisibleTargets() (например если их коллайдеры
    ///     не на Detectable layer, либо здание вне DetectionRadius/FOV на
    ///     момент последнего скана).
    /// </summary>
    public static class TargetingUtility
    {
        // ── Primary API (Raid — без изменений) ─────────────────────────────

        public static ITargetInfo FindNearestHostile(UnitInstance unit)
        {
            var targets = unit.PhysicsPerception.GetVisibleTargets();
            var self = GetRuntime(unit);

            ITargetInfo nearest = null;
            float bestSqDist = float.MaxValue;

            foreach (var t in targets)
            {
                if (t.IsDead) continue;
                if (!TeamService.CanDamage(self, t.Unit?.RuntimeBase)) continue;

                float d = (t.Position - unit.transform.position).sqrMagnitude;
                if (d < bestSqDist)
                {
                    bestSqDist = d;
                    nearest = t;
                }
            }

            return nearest;
        }

        /// <summary>
        /// NEW — nearest visible hostile PLAYER unit only, excludes facilities
        /// (HQ, walls, etc). Используется Siege AI: здания обрабатываются
        /// отдельно через SiegePathService, не через общий hostile pipeline.
        /// </summary>
        public static ITargetInfo FindNearestHostilePlayer(UnitInstance unit)
        {
            var targets = unit.PhysicsPerception.GetVisibleTargets();
            var self = GetRuntime(unit);

            ITargetInfo nearest = null;
            float bestSqDist = float.MaxValue;

            foreach (var t in targets)
            {
                if (t.IsDead) continue;
                if (t.Unit is not ISceneUnit) continue; // отсекаем здания
                if (!TeamService.CanDamage(self, t.Unit?.RuntimeBase)) continue;

                float d = (t.Position - unit.transform.position).sqrMagnitude;
                if (d < bestSqDist)
                {
                    bestSqDist = d;
                    nearest = t;
                }
            }

            return nearest;
        }

        public static ITargetInfo FindNearestHostileInRange(UnitInstance unit, float range)
        {
            var targets = unit.PhysicsPerception.GetVisibleTargets();
            var self = GetRuntime(unit);
            float rangeSq = range * range;

            ITargetInfo nearest = null;
            float bestSqDist = float.MaxValue;

            foreach (var t in targets)
            {
                if (t.IsDead) continue;
                if (!TeamService.CanDamage(self, t.Unit?.RuntimeBase)) continue;

                float d = (t.Position - unit.transform.position).sqrMagnitude;
                if (d > rangeSq) continue;
                if (d < bestSqDist)
                {
                    bestSqDist = d;
                    nearest = t;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Look up a specific hostile by id. Используется для player-issued
        /// AttackCommand(targetId) И для Siege AttackCommand(wallId/hqId).
        ///
        /// Fast path — PhysicsPerception (как раньше). Fallback (NEW) —
        /// TargetInfoRegistry.TryGetById для целей вне perception-скана
        /// (здания). LOS/FOV здесь не проверяется — приемлемо для
        /// melee-range Siege-целей, но не годится для ranged-валидации
        /// видимости на дистанции.
        /// </summary>
        public static ITargetInfo FindHostileById(UnitInstance unit, string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;

            var self = GetRuntime(unit);

            var target = unit.PhysicsPerception.GetTargetById(targetId);

            if (target == null)
                TargetInfoRegistry.TryGetById(targetId, out target); // NEW fallback

            if (target == null || target.IsDead) return null;
            if (!TeamService.CanDamage(self, target.Unit?.RuntimeBase)) return null;

            return target;
        }

        public static bool HasVisibleHostile(UnitInstance unit)
        {
            var targets = unit.PhysicsPerception.GetVisibleTargets();
            var self = GetRuntime(unit);

            foreach (var t in targets)
            {
                if (!t.IsDead && TeamService.CanDamage(self, t.Unit?.RuntimeBase))
                    return true;
            }

            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static IUnitRuntimeBase GetRuntime(UnitInstance unit)
        {
            if (unit.EnemyAdapter != null)
                return unit.EnemyAdapter.RuntimeBase;

            return unit.UnitAdapter?.RuntimeBase;
        }
    }
}
