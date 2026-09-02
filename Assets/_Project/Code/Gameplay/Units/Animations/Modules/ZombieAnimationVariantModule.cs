using System.Collections.Generic;
using Galactic1.Code.Gameplay.Animation.Zombie;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Variants
{
    /// <summary>
    /// Runtime animation variant resolver for zombies.
    ///
    /// Two responsibilities:
    ///
    ///   1. ACTION VARIANTS (per-event, lightweight)
    ///      Maintains a count-per-type map so attack/death modules
    ///      can call GetVariant() to receive a random index into their hash arrays.
    ///
    ///   2. LOCOMOTION AOC (spawn-time, one-shot)
    ///      Creates an independent AnimatorOverrideController per zombie instance,
    ///      replaces the canonical Idle/Walk/Run clips with archetype-specific ones,
    ///      and assigns it to the Animator once. Zero per-frame cost afterward.
    ///
    /// Does NOT:
    ///   — Update the animator per frame
    ///   — Control any animator state or trigger transitions
    ///   — Contain AI or FSM logic
    /// </summary>
    public sealed class ZombieAnimationVariantModule : MonoBehaviour, IAnimationVariantModule
    {
        // =========================================================
        // Action variant map
        // =========================================================

        private readonly Dictionary<AnimationVariantType, int> _map = new();

        // =========================================================
        // Init
        // =========================================================

        /// <summary>
        /// Called once at spawn by ZombieInstance.
        /// Builds the action variant map and applies the locomotion AOC.
        /// </summary>
        public void Initialize(
            IReadOnlyList<AnimationVariantConfig.Entry> variantEntries,
            AnimatorOverrideController overrideController,
            ZombieAnimConfig animConfig,
            Animator animator)
        {
            BuildVariantMap(variantEntries);
            SetOverrideController(animator, overrideController);
        }

        // =========================================================
        // IAnimationVariantModule
        // =========================================================

        /// <summary>
        /// Returns a random variant index for the given action type.
        /// Used by ZombieAttackAnimationModule, ZombieDeathAnimationModule.
        /// </summary>
        public int GetVariant(AnimationVariantType type)
        {
            if (_map.TryGetValue(type, out int count))
                return Random.Range(0, count);

            return 0;
        }

        // =========================================================
        // Action variant map
        // =========================================================

        private void BuildVariantMap(IReadOnlyList<AnimationVariantConfig.Entry> entries)
        {
            _map.Clear();

            if (entries == null) return;

            foreach (var e in entries)
                _map[e.Type] = Mathf.Max(1, e.Count);
        }

        // =========================================================
        // Locomotion AOC — one-shot at spawn
        // =========================================================

        public void SetOverrideController(Animator animator, AnimatorOverrideController controller)
        {
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
        }
    }
}