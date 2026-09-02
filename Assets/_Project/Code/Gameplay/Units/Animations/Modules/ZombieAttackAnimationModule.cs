using Galactic1.Code.Gameplay.Animation.Variants;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Zombie
{
    /// <summary>
    /// Zombie-specific melee attack animation module.
    ///
    /// Responsibilities:
    /// - Select attack animation variants
    /// - Trigger attack animations
    /// - Route animator attack states
    ///
    /// Does NOT:
    /// - Apply gameplay damage
    /// - Control FSM
    /// - Detect targets
    /// </summary>
    [RequireComponent(typeof(AnimatorBridge))]
    public sealed class ZombieAttackAnimationModule : 
        MonoBehaviour,
        IAttackAnimationModule
    {
        // =========================================================
        // Config
        // =========================================================

        [SerializeField] private float attackCrossFade = 0.05f;

        // =========================================================
        // Deps
        // =========================================================

        private ZombieAnimConfig _config;
        private AnimatorBridge _bridge;
        private IAnimationVariantModule _variants;

        // =========================================================
        // State
        // =========================================================

        private bool _isDead;

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
        /// Plays zombie melee attack animation.
        /// </summary>
        public void PlayAttack()
        {
            PlayMeleeAttack();
        }
        public void PlayMeleeAttack()
        {
            if (_isDead)
                return;
            
            _bridge.CrossFade(
                ResolveAttackHash(ResolveVariant()),
                attackCrossFade, 
                0);
        }
        
        public void PlayRangedAttack()
        {
            // zombies do not support ranged
        }

        /// <summary>
        /// Called when zombie dies.
        /// </summary>
        public void MarkDead()
        {
            _isDead = true;
        }

        /// <summary>
        /// Reset for pooling/revive.
        /// </summary>
        public void ResetState()
        {
            _isDead = false;
        }

        // =========================================================
        // Variant
        // =========================================================

        private int ResolveVariant()
        {
            if (_variants == null)
                return 0;

            return _variants.GetVariant(AnimationVariantType.Attack);
        }
        
        private int ResolveAttackHash(int variant)
            => _config.AttackHashes[Mathf.Clamp(variant, 0,  _config.AttackHashes.Length - 1)];

        // =========================================================
        // DEV
        // =========================================================


        
        public void Debug_PlayAttack(int variant)
        {
            _bridge.CrossFade(ResolveAttackHash(variant), attackCrossFade, 0);
        }


    }
}