using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Pure combat validation. No target acquisition, no hostile filtering.
    ///
    /// WeaponSlot removed — callers supply range explicitly.
    /// This makes CombatLogic work identically for melee and ranged without
    /// any weapon-specific branching.
    ///
    /// Team filtering lives in TargetingUtility. This class never calls TeamService.
    /// </summary>
    public sealed class UnitCombatLogic : MonoBehaviour, ICombatLogic
    {
        private IPerception _perception;

        // Eye height offset applied to all LOS / range checks
        private const float EyeHeightOffset = 1.4f;

        public void Initialize(
            IPerception perception,
            WeaponSlot weaponSlot,       // kept for API compat; no longer stored
            IUnitSceneContext self)       // kept for API compat; no longer stored
        {
            _perception = perception;
            // weaponSlot and self are intentionally not stored.
            // Target acquisition (FindBestTarget, GetTarget) has moved to TargetingUtility.
            // Hostility checks (TeamService) have moved to TargetingUtility.
        }

        // ── ICombatLogic ──────────────────────────────────────────────────

        /// <summary>
        /// Full combat legality check: alive + in range + LOS.
        /// Caller is responsible for ensuring the target is hostile
        /// (obtain via TargetingUtility before calling this).
        /// </summary>
        public bool CanAttack(UnitInstance unit, ITargetInfo target, float range)
        {
            if (target == null || target.IsDead) return false;
            if (!IsInRange(unit, target, range)) return false;
            return HasLineOfSight(unit, target);
        }

        /// <summary>
        /// Distance + alive check only. No LOS.
        /// Useful for melee pre-check before committing to movement.
        /// </summary>
        public bool IsInRange(UnitInstance unit, ITargetInfo target, float range)
        {
            if (target == null || target.IsDead) 
                return false;
            
            return (target.AimPoint - EyePoint(unit)).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Unobstructed LOS from unit eye point to target.
        /// Delegates to PhysicsPerception — single source of truth for raycasting.
        /// </summary>
        public bool HasLineOfSight(UnitInstance unit, ITargetInfo target)
        {
            if (target == null || target.IsDead)
                return false;

            return _perception.HasLineOfSight(
                EyePoint(unit),
                target.AimPoint);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static Vector3 EyePoint(UnitInstance unit)
            => unit.transform.position + Vector3.up * EyeHeightOffset;
    }
}