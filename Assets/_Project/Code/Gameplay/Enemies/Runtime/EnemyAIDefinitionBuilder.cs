using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Конвертирует authoring List&lt;ActionWeightEntry&gt; в immutable runtime EnemyAIDefinition.
    ///
    /// Правила:
    ///   — Weight клампуется в [0, 5] — designer не может сломать баланс.
    ///   — Если ActionWeights пуст — все известные action'ы получают weight=1, enabled=true.
    ///   — Неизвестные AIActionType в записях игнорируются (forward-compatibility).
    /// </summary>
    public static class EnemyAIDefinitionBuilder
    {
        private const float MinWeight = 0f;
        private const float MaxWeight = 5f;

        public static EnemyAIDefinition Build(EnemyAIConfig config)
        {
            var actions = BuildActionDictionary(config.ActionWeights);

            return new EnemyAIDefinition(
                config.ThinkInterval,
                config.RoamRadius,
                config.WaypointRadius,
                config.UsePackBehaviour,
                actions);
        }

        private static Dictionary<AIActionType, AIActionDefinition> BuildActionDictionary(
            List<ActionWeightEntry> entries)
        {
            var result = new Dictionary<AIActionType, AIActionDefinition>();

            if (entries == null || entries.Count == 0)
            {
                // Нет конфига — все actions включены с weight=1
                FillDefaults(result);
                return result;
            }

            foreach (var entry in entries)
            {
                float clampedWeight = Mathf.Clamp(entry.Weight, MinWeight, MaxWeight);
                result[entry.Action] = new AIActionDefinition(entry.Action, clampedWeight, entry.Enabled);
            }

            // Для action'ов без записи — добавляем дефолт чтобы Brain их не пропускал
            FillMissingWithDefaults(result);

            return result;
        }

        /// <summary>
        /// Заполняет все известные action'ы дефолтными значениями (weight=1, enabled=true).
        /// </summary>
        private static void FillDefaults(Dictionary<AIActionType, AIActionDefinition> result)
        {
            foreach (AIActionType type in System.Enum.GetValues(typeof(AIActionType)))
                result[type] = new AIActionDefinition(type, weight: 1f, enabled: true);
        }

        /// <summary>
        /// Добавляет дефолт только для тех action'ов которых нет в конфиге.
        /// Так добавление новых AIActionType не требует обновления всех конфигов.
        /// </summary>
        private static void FillMissingWithDefaults(Dictionary<AIActionType, AIActionDefinition> result)
        {
            foreach (AIActionType type in System.Enum.GetValues(typeof(AIActionType)))
            {
                if (!result.ContainsKey(type))
                    result[type] = new AIActionDefinition(type, weight: 1f, enabled: true);
            }
        }
    }
}