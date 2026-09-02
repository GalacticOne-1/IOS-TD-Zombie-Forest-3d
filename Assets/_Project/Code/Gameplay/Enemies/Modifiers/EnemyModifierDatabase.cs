
using System;
using System.Collections.Generic;
using Galactic1.Configs;
using Galactic1.Game.Meta.Enemy.Modifiers;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    public sealed class EnemyModifierDatabase
    {
        // Factory registry:
        // modifierId -> runtime modifier creator
        private readonly Dictionary<string, Func<IEnemyModifier>> _factories = new();


        public EnemyModifierDatabase(ConfigProvider configProvider)
        {
            var factory = new EnemyModifierFactory();
            
            var configs = configProvider.Get<EnemyModifierConfigs>().ModifierConfigs;
            foreach (var c in configs)
            {
                RegisterFromConfig(c, factory);
            }
        }

        /// <summary>
        /// Registers custom runtime factory.
        /// Useful for dependency-injected modifiers.
        /// </summary>
        public void Register(
            string modifierId,
            Func<IEnemyModifier> factory)
        {
            if (string.IsNullOrWhiteSpace(modifierId))
            {
                Debug.LogError("[EnemyModifierDatabase] ModifierId is null or empty.");
                return;
            }

            if (factory == null)
            {
                Debug.LogError($"[EnemyModifierDatabase] Factory is null for '{modifierId}'.");
                return;
            }

            _factories[modifierId] = factory;
        }

        /// <summary>
        /// Registers data-driven modifier config.
        /// Runtime modifier instances are created lazily per spawn.
        /// </summary>
        public void RegisterFromConfig(
            EnemyModifierConfig config,
            EnemyModifierFactory factory)
        {
            if (config == null)
            {
                Debug.LogError("[EnemyModifierDatabase] Config is null.");
                return;
            }

            if (factory == null)
            {
                Debug.LogError("[EnemyModifierDatabase] Factory is null.");
                return;
            }

            Register(
                config.ModifierId,
                () => factory.CreateFromConfig(config));
        }

        // ─────────────────────────────────────────────────────────────
        // Runtime Creation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates NEW runtime modifier instance.
        /// Never returns shared singleton modifier.
        /// </summary>
        public bool TryCreate(
            string modifierId,
            out IEnemyModifier modifier)
        {
            if (_factories.TryGetValue(modifierId, out var factory))
            {
                modifier = factory();
                return true;
            }

            Debug.LogError(
                $"[EnemyModifierDatabase] Modifier not found: '{modifierId}'");

            modifier = null;
            return false;
        }

        /// <summary>
        /// Resolves modifier ids into fresh runtime modifier instances.
        /// Invalid ids are skipped safely.
        /// </summary>
        public List<IEnemyModifier> Resolve(
            IReadOnlyList<string> modifierIds)
        {
            if (modifierIds == null || modifierIds.Count == 0)
                return EmptyList<IEnemyModifier>.Value;

            var result = new List<IEnemyModifier>(modifierIds.Count);

            foreach (var id in modifierIds)
            {
                if (TryCreate(id, out var modifier))
                    result.Add(modifier);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────
        // Debug / Validation
        // ─────────────────────────────────────────────────────────────

        public bool Contains(string modifierId)
            => _factories.ContainsKey(modifierId);

        public int Count => _factories.Count;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Utility
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    internal static class EmptyList<T>
    {
        public static readonly List<T> Value = new(0);
    }
}