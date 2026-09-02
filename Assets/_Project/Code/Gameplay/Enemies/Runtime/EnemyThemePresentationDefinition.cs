
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Enemies.Definitions
{
    /// <summary>
    /// Иммутабельный снапшот одной визуальной темы.
    /// Weight намеренно отсутствует — он хранится в EnemyVisualRulesDefinition.
    /// </summary>
    public sealed class EnemyThemePresentationDefinition
    {
        public EnemyVisualThemeId ThemeId { get; }
        public string PrefabPrefix { get; }
        public int VariantsCount { get; }

        public EnemyThemePresentationDefinition(
            EnemyVisualThemeId themeId,
            string prefabPrefix,
            int variantsCount)
        {
            ThemeId = themeId;
            PrefabPrefix = prefabPrefix;
            VariantsCount = variantsCount;
        }

        /// <summary>
        /// Строит PrefabId для конкретного индекса.
        /// Пример: PrefabPrefix="civil_", index=3 → "civil_03"
        /// </summary>
        public string BuildPrefabId(int index) => $"{PrefabPrefix}{index:00}";
    }
}