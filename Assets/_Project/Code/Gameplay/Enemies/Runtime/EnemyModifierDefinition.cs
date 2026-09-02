
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Иммутабельное runtime-определение модификатора врага.
    ///
    /// Создаётся EnemyModifierFactory из EnemyModifierConfig (SO).
    /// Хранится в EnemyModifierDatabase.
    ///
    /// Слой:
    ///   EnemyModifierConfig (authoring SO)
    ///     ↓  EnemyModifierFactory
    ///   EnemyModifierDefinition (runtime)
    ///     ↓  DataDrivenModifier
    ///   EnemyStatMutationContext (применение)
    /// </summary>
    public sealed class EnemyModifierDefinition
    {
        /// <summary>Уникальный ID модификатора.</summary>
        public string ModifierId { get; }

        /// <summary>Читаемое название для логов и дебага.</summary>
        public string DisplayName { get; }

        /// <summary>Множители статов. Применяются к значениям EnemyStatMutationContext.Stats.</summary>
        public IReadOnlyList<StatMultiplierEntry> StatMultipliers { get; }

        /// <summary>Множитель скорости ходьбы. 1.0 = без изменений.</summary>
        public float WalkSpeedMultiplier { get; }

        /// <summary>Множитель скорости бега. 1.0 = без изменений.</summary>
        public float RunSpeedMultiplier { get; }

        /// <summary>Оверрайд PrefabId. Пустой = базовый prefab.</summary>
        public string PrefabIdOverride { get; }

        /// <summary>Оверрайд аниматора. null = базовый аниматор.</summary>
        public UnityEngine.RuntimeAnimatorController AnimatorOverride { get; }

        /// <summary>Устанавливает флаг IsElite в EnemyStatMutationContext.</summary>
        public bool SetsEliteFlag { get; }

        /// <summary>Добавка к ThreatMultiplier.</summary>
        public float ThreatMultiplierBonus { get; }

        public EnemyModifierDefinition(
            string modifierId,
            string displayName,
            IReadOnlyList<StatMultiplierEntry> statMultipliers,
            float walkSpeedMultiplier,
            float runSpeedMultiplier,
            string prefabIdOverride,
            UnityEngine.RuntimeAnimatorController animatorOverride,
            bool setsEliteFlag,
            float threatMultiplierBonus)
        {
            ModifierId = modifierId;
            DisplayName = displayName;
            StatMultipliers = statMultipliers;
            WalkSpeedMultiplier = walkSpeedMultiplier;
            RunSpeedMultiplier = runSpeedMultiplier;
            PrefabIdOverride = prefabIdOverride;
            AnimatorOverride = animatorOverride;
            SetsEliteFlag = setsEliteFlag;
            ThreatMultiplierBonus = threatMultiplierBonus;
        }

        public readonly struct StatMultiplierEntry
        {
            public readonly StatId StatId;
            public readonly float Multiplier;

            public StatMultiplierEntry(StatId statId, float multiplier)
            {
                StatId = statId;
                Multiplier = multiplier;
            }
        }
    }
}