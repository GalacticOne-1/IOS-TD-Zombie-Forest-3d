
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Definitions;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Variants
{
    /// <summary>
    /// Выбирает визуальный скин врага.
    ///
    /// Pipeline выбора:
    ///   1. Отфильтровать Themes[] по правилам локации
    ///   2. Выбрать тему weighted random по весам ИЗ ЛОКАЦИИ
    ///   3. Сгенерировать индекс [1..VariantsCount]
    ///   4. Построить PrefabId: prefix + index
    ///
    /// Weight берётся из EnemyVisualRulesDefinition (локация),
    /// а НЕ из EnemyThemePresentationDefinition (враг).
    /// </summary>
    public sealed class EnemyVariantResolver
    {
        public EnemyVariantResolveResult Resolve(
            EnemyPresentationDefinitionData presentation,
            EnemyVariantResolveContext context)
        {
            var rules = context.VisualRules;

            // Нет тем — базовый visual prefab
            if (presentation.Themes == null || presentation.Themes.Count == 0)
                return FallbackToBaseVisual(presentation);

            // Шаг 1: фильтруем по правилам локации
            var allowed = BuildAllowedThemes(presentation.Themes, rules);

            if (allowed.Count == 0)
            {
                // Ни одна тема не разрешена — используем базовый visual prefab
                Debug.LogWarning(
                    "[EnemyVariantResolver] Нет разрешённых тем для локации. " +
                    "BaseVisualId.");
                return FallbackToBaseVisual(presentation);
            }

            // Шаг 2: weighted random по весам локации
            var picked = WeightedRandomByLocationWeights(allowed, rules);

            // Шаг 3: случайный индекс [1..VariantsCount]
            int index = Random.Range(1, picked.VariantsCount + 1);

            // Шаг 4: строим PrefabId
            string prefabId = picked.BuildPrefabId(index);

#if UNITY_EDITOR
            Debug.Log(
                $"[EnemyVariantResolver] Тема={picked.ThemeId?.DebugKey} " +
                $"| Индекс={index} | PrefabId={prefabId}");
#endif

            return EnemyVariantResolveResult.Resolved(
                picked.ThemeId?.DebugKey ?? string.Empty,
                prefabId);
        }

        // ── Приватные методы ──────────────────────────────────────────

        private static EnemyVariantResolveResult FallbackToBaseVisual(
            EnemyPresentationDefinitionData presentation)
        {
            if (!string.IsNullOrEmpty(presentation.BaseVisualId))
                return EnemyVariantResolveResult.FallbackUsed(
                    string.Empty,
                    presentation.BaseVisualId,
                    "Темы недоступны — BaseVisualId.");

            return EnemyVariantResolveResult.DefaultRequired(
                "Темы недоступны и BaseVisualId не задан.");
        }

        /// <summary>
        /// Фильтрует темы врага по правилам локации.
        /// Тема проходит фильтр если она есть в словаре весов локации.
        /// </summary>
        private static List<EnemyThemePresentationDefinition> BuildAllowedThemes(
            IReadOnlyList<EnemyThemePresentationDefinition> themes,
            EnemyVisualRulesDefinition rules)
        {
            var result = new List<EnemyThemePresentationDefinition>();
            foreach (var t in themes)
            {
                if (rules == null || rules.IsThemeAllowed(t.ThemeId))
                    result.Add(t);
            }

            return result;
        }

        /// <summary>
        /// Weighted random с весами ИЗ ЛОКАЦИИ, а не из темы врага.
        /// </summary>
        private static EnemyThemePresentationDefinition WeightedRandomByLocationWeights(
            List<EnemyThemePresentationDefinition> themes,
            EnemyVisualRulesDefinition rules)
        {
            float total = 0f;
            foreach (var t in themes)
                total += rules?.GetWeight(t.ThemeId) ?? 1f;

            if (total <= 0f) return themes[0];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var t in themes)
            {
                cumulative += rules?.GetWeight(t.ThemeId) ?? 1f;
                if (roll <= cumulative) return t;
            }

            return themes[themes.Count - 1];
        }
    }
}