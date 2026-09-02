
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Enemies.Variants
{
    /// <summary>
    /// Иммутабельный runtime-снапшот правил визуальных тем.
    ///
    /// Хранит словарь ThemeId → Weight.
    /// Null означает отсутствие ограничений.
    ///
    /// Используется EnemyVariantResolver для:
    ///   1. Фильтрации тем (только те что есть в словаре)
    ///   2. Weighted random выбора (по весам из словаря)
    /// </summary>
    public sealed class EnemyVisualRulesDefinition
    {
        /// <summary>
        /// Словарь разрешённых тем и их весов.
        /// null = без ограничений, все темы разрешены с равным весом.
        /// </summary>
        private readonly Dictionary<EnemyVisualThemeId, float> _themeWeights;

        public bool IsUnrestricted => _themeWeights == null;

        private EnemyVisualRulesDefinition(
            Dictionary<EnemyVisualThemeId, float> themeWeights)
        {
            _themeWeights = themeWeights;
        }

        public static EnemyVisualRulesDefinition Unrestricted() => new(null);

        public static EnemyVisualRulesDefinition FromWeightMap(
            Dictionary<EnemyVisualThemeId, float> weights) => new(weights);

        /// <summary>Разрешена ли тема на этой локации.</summary>
        public bool IsThemeAllowed(EnemyVisualThemeId theme)
        {
            if (IsUnrestricted) return true;
            if (theme == null) return false;
            return _themeWeights.ContainsKey(theme);
        }

        /// <summary>
        /// Возвращает вес темы для weighted random.
        /// Если тема не задана в локации — возвращает 0.
        /// Если правила отсутствуют (unrestricted) — возвращает 1 для всех.
        /// </summary>
        public float GetWeight(EnemyVisualThemeId theme)
        {
            if (IsUnrestricted) return 1f;
            if (theme == null) return 0f;
            return _themeWeights.TryGetValue(theme, out var w) ? w : 0f;
        }
    }
}