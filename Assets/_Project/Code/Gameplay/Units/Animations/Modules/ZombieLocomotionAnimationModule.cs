using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Code.Systems.Squad;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    /// <summary>
    /// Zombie locomotion animation driver.
    ///
    /// Responsibilities (after refactor):
    ///   — Set Speed parameter each frame
    ///   — Set IsMoving parameter each frame
    ///
    /// Does NOT:
    ///   — Call CrossFade / CrossFadeFixed
    ///   — Arbitrate locomotion states
    ///   — Select variants (→ ZombieAnimationVariantModule at spawn)
    ///   — Interrupt or own any animator state
    ///
    /// The BlendTree (Locomotion_BT) evaluates Speed continuously.
    /// This module is a pure parameter pump — it cannot interrupt attack,
    /// hit, or death animations because it never triggers transitions.
    /// </summary>
    [RequireComponent(typeof(AnimatorBridge))]
    [RequireComponent(typeof(UnitMover))]
    public sealed class ZombieLocomotionAnimationModule : MonoBehaviour, ILocomotionAnimationModule
    {
        // =========================================================
        // Config
        // =========================================================

        [Header("Thresholds")] [SerializeField]
        private float idleThreshold = 0.1f;

        // =========================================================
        // Deps
        // =========================================================

        private AnimatorBridge _bridge;
        private UnitMover _mover;
        private ZombieAnimConfig _config;

        private float _maxLocomotionSpeed;

        // =========================================================
        // Init
        // =========================================================

        public void Initialize(BaseAnimConfig config, UnitGameplayDefinition definition)
        {
            _config = config as ZombieAnimConfig;
            _bridge = GetComponent<AnimatorBridge>();
            _mover = GetComponent<UnitMover>();

            _maxLocomotionSpeed = ((EnemyRuntimeDefinition)definition).MovementDefinition.RunSpeed;
        }

        // =========================================================
        // Tick
        // =========================================================

        public void Tick()
        {
#if UNITY_EDITOR
            if (_debugMode) return;
#endif
            UpdateLocomotion();
        }

        // =========================================================
        // Locomotion — parameter pump only
        // =========================================================

        private void UpdateLocomotion()
        {
            float speed = _mover.Velocity.magnitude;
            float normalized =
                Mathf.Clamp01(speed / _maxLocomotionSpeed);

            _bridge.SetFloat(_config.SpeedHash, normalized);
            _bridge.SetBool(_config.IsMovingHash, speed > idleThreshold);
        }

        // =========================================================
        // Public API
        // =========================================================

        /// <summary>
        /// Forces a locomotion parameter reset (revive / pool reuse).
        /// No animator state routing needed — BlendTree self-corrects next frame.
        /// </summary>
        public void ResetLocomotion()
        {
            _bridge.SetFloat(_config.SpeedHash, 0f);
            _bridge.SetBool(_config.IsMovingHash, false);
        }

        // =========================================================
        // DEV
        // =========================================================


        private bool _debugMode;

        /// <summary>Forces a specific speed value for preview purposes.</summary>
        public void Debug_SetSpeed(float speed)
        {
            _debugMode = true;
            _bridge.SetFloat(_config.SpeedHash, speed);
            _bridge.SetBool(_config.IsMovingHash, speed > idleThreshold);
        }

        public void Debug_Stop()
        {
            _debugMode = false;
            _bridge.SetFloat(_config.SpeedHash, 0f);
            _bridge.SetBool(_config.IsMovingHash, false);
        }

    }
}