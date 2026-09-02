#if UNITY_EDITOR
using Galactic1.Code.Gameplay.Units;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Gameplay
{
    /// <summary>
    /// Unified combat range visualization for any UnitInstance.
    ///
    /// Attach to the same GameObject as UnitInstance (or its subclass).
    /// Draws all combat ranges in a consistent color convention:
    ///
    ///   YELLOW      Detection radius       (PhysicsPerception.DetectionRadius)
    ///   CYAN        Hearing radius         (PhysicsPerception.HearingRadius)
    ///   MAGENTA     AI engage range        (EnemyCombatDefinition.AttackRange)
    ///   ORANGE      FSM attack range       (MeleeAttackComponent.AttackRange)
    ///   RED         Hit range              (MeleeAttackComponent.HitRange)
    ///   WHITE dot   Hit origin             (MeleeAttackComponent.ResolveHitOrigin())
    ///   GREEN line  LOS to current target  (TargetingUtility.FindNearestHostile)
    ///
    /// All drawing is #if UNITY_EDITOR — zero runtime cost.
    /// Toggle individual channels via serialized bool fields (visible in Inspector).
    ///
    /// Does NOT access ScriptableObjects. Reads from PhysicsPerception and
    /// MeleeAttackComponent which are already initialized runtime components.
    /// </summary>
    [RequireComponent(typeof(UnitInstance))]
    public sealed class UnitCombatGizmos : MonoBehaviour
    {
        // =========================================================
        // Toggles (Inspector)
        // =========================================================

        [Header("Detection")]
        [SerializeField] private bool showDetectionRadius = true;
        [SerializeField] private bool showHearingRadius   = true;

        [Header("Combat Ranges")]
        [SerializeField] private bool showEngageRange  = true;
        [SerializeField] private bool showAttackRange  = true;
        [SerializeField] private bool showHitRange     = true;
        [SerializeField] private bool showHitOrigin    = true;

        [Header("Targeting")]
        [SerializeField] private bool showCurrentTarget = true;
        [SerializeField] private bool showLOS           = true;

        // =========================================================
        // Color convention (static — same across all units)
        // =========================================================

        private static readonly Color ColorDetection = Color.yellow;
        private static readonly Color ColorHearing   = new(0f, 0.8f, 1f, 0.35f);
        private static readonly Color ColorEngage    = new(1f, 0f, 1f, 0.6f);   // magenta
        private static readonly Color ColorAttack    = new(1f, 0.5f, 0f, 0.8f); // orange
        private static readonly Color ColorHit       = new(1f, 0.15f, 0.15f, 0.9f); // red
        private static readonly Color ColorHitOrigin = Color.white;
        private static readonly Color ColorTargetLOS = Color.green;
        private static readonly Color ColorTargetLOSBlocked = new(1f, 0.3f, 0f, 0.8f);

        // =========================================================
        // Gizmos
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            var unit = GetComponent<UnitInstance>();
            if (unit == null) return;

            Vector3 origin = transform.position;
            Vector3 eyePoint = origin; // fallback; PhysicsPerception may refine this

            // ── Detection / Hearing ────────────────────────────────────────
            var perception = unit.PhysicsPerception;
            if (perception != null)
            {
                if (showDetectionRadius)
                {
                    Gizmos.color = ColorDetection;
                    Gizmos.DrawWireSphere(origin, perception.Def.DetectionRadius);
                }

                if (showHearingRadius)
                {
                    Gizmos.color = ColorHearing;
                    Gizmos.DrawWireSphere(origin, perception.Def.HearingRadius);
                }
            }

            // ── Melee ranges ───────────────────────────────────────────────
            var melee = unit.MeleeAttack;
            if (melee != null)
            {
                // AI engage range — where brain decides to attack
                // (drawn at unit feet level, same as how AI measures distance)
                if (showEngageRange)
                {
                    Gizmos.color = ColorEngage;
                    Gizmos.DrawWireSphere(origin, melee.AttackRange);
                    Handles.color = ColorEngage;
                    Handles.Label(origin + Vector3.up * (melee.AttackRange + 0.15f),
                        $"Attack: {melee.AttackRange:F2}m");
                }

                Vector3 hitOrigin = melee.ResolveHitOrigin();

                // FSM attack range — drawn from hit origin (correct contact height)
                if (showAttackRange)
                {
                    Gizmos.color = ColorAttack;
                    Gizmos.DrawWireSphere(hitOrigin, melee.AttackRange);
                }

                // Hit detection sphere — the actual OverlapSphere on animation event
                if (showHitRange)
                {
                    Gizmos.color = ColorHit;
                    Gizmos.DrawWireSphere(hitOrigin, melee.HitRange);
                    Handles.color = ColorHit;
                    Handles.Label(hitOrigin + Vector3.up * (melee.HitRange + 0.1f),
                        $"Hit: {melee.HitRange:F2}m");
                }

                // Hit origin marker
                if (showHitOrigin)
                {
                    Gizmos.color = ColorHitOrigin;
                    Gizmos.DrawSphere(hitOrigin, 0.06f);

                    // Line from root to hit origin
                    Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
                    Gizmos.DrawLine(origin, hitOrigin);
                }
            }

            // ── Target / LOS ───────────────────────────────────────────────
            if (showCurrentTarget || showLOS)
            {
                // FindNearestHostile reads from the already-updated perception cache —
                // safe to call in editor since it's just iterating a list.
                var target = Galactic1.Code.Gameplay.Units.Brain.Utility.Core
                    .TargetingUtility.FindNearestHostile(unit);

                if (target != null)
                {
                    if (showCurrentTarget)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(target.Position, 0.25f);
                        Handles.color = Color.green;
                        Handles.Label(target.Position + Vector3.up * 0.4f,
                            $"Target\n{target.TargetId?[..Mathf.Min(8, target.TargetId?.Length ?? 0)]}");
                    }

                    if (showLOS && perception != null)
                    {
                        bool hasLOS = perception.HasLineOfSight(eyePoint, target.Position);
                        Gizmos.color = hasLOS ? ColorTargetLOS : ColorTargetLOSBlocked;
                        Gizmos.DrawLine(eyePoint, target.Position);
                    }
                }
            }
        }
    }
}
#endif