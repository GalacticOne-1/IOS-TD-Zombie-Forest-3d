using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Definitions
{
    /// <summary>
    /// Immutable runtime melee combat definition.
    ///
    /// Range semantics — three distinct values, never conflated:
    ///
    ///   AttackRange       The distance at which the FSM state decides to stop
    ///                     moving and begin the attack animation. This is the
    ///                     "reach" of the unit — arm length + a small buffer.
    ///                     Used by: ZombieMeleeEngagingState.Tick()
    ///
    ///   HitRange          Radius of the OverlapSphere fired on the animation event.
    ///                     Should be <= AttackRange. Slightly tighter is intentional:
    ///                     attack range is "I'm close enough to try", hit range is
    ///                     "my fist actually connects".
    ///                     Used by: MeleeAttackComponent.OnAnimationMeleeHitEvent()
    ///
    ///   HitOriginOffset   Local-space offset from the unit root applied at hit
    ///                     detection time. Moves the OverlapSphere from root (feet)
    ///                     to the actual contact point (hand / weapon tip / torso).
    ///                     Expressed as (forward, up) — no Transform dependency.
    ///                     Used by: MeleeAttackComponent.OnAnimationMeleeHitEvent()
    ///
    /// Why no Transform reference for hit origin:
    ///   WeaponRigController.HitOrigin is a scene concern (rig bone position).
    ///   RuntimeDefinition must not reference scene objects.
    ///   The offset approach keeps Definition immutable and scene-independent.
    ///   For weapon-based melee (survivors), the WeaponRigController still provides
    ///   an exact Transform; that path is preserved via MeleeAttackComponent's
    ///   optional hitOriginOverride constructor parameter.
    /// </summary>
    public sealed class MeleeCombatDefinition
    {
        /// <summary>FSM swing trigger distance (arm reach + buffer).</summary>
        public float AttackRange { get; }

        /// <summary>OverlapSphere radius at hit event.</summary>
        public float HitRange { get; }

        /// <summary>
        /// Local-space hit sphere center offset from unit root.
        /// Applied as: root.position + root.forward * HitOriginOffset.z + Vector3.up * HitOriginOffset.y
        /// Defaults to (0, 1.0, 0.6) — chest height, slightly in front.
        /// </summary>
        public Vector3 HitOriginOffset { get; }

        public float Damage { get; }
        public float Cooldown { get; }
        public float ReadyToAttackAngle { get; }

        public MeleeCombatDefinition(
            float attackRange,
            float hitRange,
            Vector3 hitOriginOffset,
            float damage,
            float cooldown,
            float readyToAttackAngle = 60f)
        {
            AttackRange = attackRange;
            HitRange = hitRange;
            HitOriginOffset = hitOriginOffset;
            Damage = damage;
            Cooldown = cooldown;
            ReadyToAttackAngle = readyToAttackAngle;
        }

        // ── Convenience ctor for weapon-based melee (hit origin provided by rig) ──
        // HitOriginOffset is zero — MeleeAttackComponent uses the injected Transform instead.
        public MeleeCombatDefinition(
            float attackRange,
            float hitRange,
            float damage,
            float cooldown,
            float readyToAttackAngle = 60f)
            : this(attackRange, hitRange, Vector3.zero, damage, cooldown, readyToAttackAngle)
        {
        }
    }
}