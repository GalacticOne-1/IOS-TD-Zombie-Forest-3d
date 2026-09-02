using Galactic1.Code.Gameplay.Animation.Variants;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    /// <summary>
    /// Handles zombie death presentation logic.
    ///
    /// Responsibilities:
    /// - Select death animation variant
    /// - Trigger death animation
    /// - Control animator layer/state transitions
    /// - Support debug forcing
    ///
    /// Does NOT:
    /// - Modify gameplay state
    /// - Handle FSM logic
    /// </summary>
    [RequireComponent(typeof(AnimatorBridge))]
    public sealed class ZombieDeathAnimationModule : MonoBehaviour, IDeathAnimationModule
    {
        // =========================================================
        // Config
        // =========================================================

        [SerializeField] private float crossFade = 0.1f;

        // =========================================================
        // Deps
        // =========================================================

        private AnimatorBridge _bridge;
        private ZombieAnimConfig _config;
        private IAnimationVariantModule _variants;

        // =========================================================
        // State
        // =========================================================

        private bool _isDead;
        private int _currentVariant = -1;

        // =========================================================
        // Init
        // =========================================================

        public void Initialize(BaseAnimConfig config)
        {
            _config = config as ZombieAnimConfig;

            _bridge = GetComponent<AnimatorBridge>();
            _variants = GetComponent<IAnimationVariantModule>();
        }

        // =========================================================
        // Public API
        // =========================================================

        /// <summary>
        /// Called by UnitInstance when zombie enters dying state.
        /// </summary>
        public void PlayDeath()
        {
            if (_isDead)
                return;

            _isDead = true;

            int variant = ResolveVariant();
            _currentVariant = variant;

            _bridge.SetBool(_config.IsMovingHash, false);
            _bridge.SetFloat(_config.SpeedHash, 0f);

            _bridge.CrossFadeFixed(ResolveDeathHash(variant), crossFade, 0, 0f);
        }

        /// <summary>
        /// Forces specific death variant (debug / editor tools).
        /// </summary>
        public void ForceDeath(int variant)
        {
            _isDead = true;
            _currentVariant = variant;

            _bridge.SetBool(_config.IsMovingHash, false);
            _bridge.SetFloat(_config.SpeedHash, 0f);

            _bridge.CrossFadeFixed(ResolveDeathHash(variant), crossFade, 0, 0f);
        }

        /// <summary>
        /// Reset for pooling / reuse.
        /// </summary>
        public void ResetDeath()
        {
            _isDead = false;
            _currentVariant = -1;
        }

        // =========================================================
        // Variant
        // =========================================================

        private int ResolveVariant()
        {
            if (_variants == null)
                return 0;

            return _variants.GetVariant(AnimationVariantType.Death);
        }
        
        private int ResolveDeathHash(int variant)
            => _config.DeathHashes[Mathf.Clamp(variant, 0, _config.DeathHashes.Length - 1)];
    }
}