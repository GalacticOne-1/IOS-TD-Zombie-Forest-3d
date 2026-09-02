using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    /// <summary>
    /// Zombie animator configuration.
    ///
    /// Locomotion is now driven by a BlendTree controlled by Speed + IsMoving.
    /// No locomotion state hashes exist here — CrossFade routing is gone.
    ///
    /// Locomotion variant clips (Idle/Walk/Run) are injected at spawn via
    /// ZombieLocomotionOverrideProfile + Animator Override Controller.
    ///
    /// Action animations (Attack, Death) remain on SSM — hashes preserved.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ZombieAnimConfig",
        menuName = "Game Configs/Enemy/Zombie Anim Config")]
    public sealed class ZombieAnimConfig : BaseAnimConfig
    {
        // =========================================================
        // Canonical clip names — must match base animator exactly.
        // Used as keys for Animator Override Controller lookups.
        // =========================================================

        [Header("Canonical Locomotion Clip Names (AOC keys)")]
        [Tooltip("Must match the clip name inside the BlendTree in the base animator.")]
        public string CanonicalIdleClip = "Idle";

        [Tooltip("Must match the clip name inside the BlendTree in the base animator.")]
        public string CanonicalWalkClip = "Walk";

        [Tooltip("Must match the clip name inside the BlendTree in the base animator.")]
        public string CanonicalRunClip  = "Run";

        // =========================================================
        // Attack SSM
        // =========================================================

        [Header("States (Attack SSM)")]
        public string attack0 = "Attack_SSM.Attack_0";
        public string attack1 = "Attack_SSM.Attack_1";
        public string attack2 = "Attack_SSM.Attack_2";
        public string attack3 = "Attack_SSM.Attack_3";
        public string attack4 = "Attack_SSM.Attack_4";
        public string attack5 = "Attack_SSM.Attack_5";

        // =========================================================
        // Death SSM
        // =========================================================

        [Header("States (Death SSM)")]
        public string death0 = "Death_SSM.Death_0";
        public string death1 = "Death_SSM.Death_1";
        public string death2 = "Death_SSM.Death_2";

        // =========================================================
        // Cached Hashes (actions only)
        // =========================================================

        [System.NonSerialized] public int[] AttackHashes;
        [System.NonSerialized] public int[] DeathHashes;

        // =========================================================
        // Init
        // =========================================================

        protected override void OnEnable()
        {
            base.OnEnable();

            AttackHashes = new[]
            {
                Animator.StringToHash(attack0),
                Animator.StringToHash(attack1),
                Animator.StringToHash(attack2),
                Animator.StringToHash(attack3),
                Animator.StringToHash(attack4),
                Animator.StringToHash(attack5),
            };

            DeathHashes = new[]
            {
                Animator.StringToHash(death0),
                Animator.StringToHash(death1),
                Animator.StringToHash(death2),
            };

            // No locomotion hashes — BlendTree is parameter-driven, not state-driven.
        }
    }
}