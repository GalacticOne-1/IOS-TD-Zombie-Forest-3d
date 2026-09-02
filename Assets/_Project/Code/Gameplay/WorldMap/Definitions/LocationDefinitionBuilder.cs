
using Galactic1.Code.Gameplay.Enemies.Variants;
using Galactic1.Code.WorldMap;

namespace Galactic1.Code.WorldMap.Definitions
{
    public sealed class LocationDefinitionBuilder
    {
        public LocationDefinition Build(LocationConfig config)
        {
            return new LocationDefinition(
                config.Id,
                BuildEnemyVisualRules(config));
        }

        private static EnemyVisualRulesDefinition BuildEnemyVisualRules(LocationConfig config)
        {
            var rules = config.EnemyVisualRules;

            if (rules == null)
                return EnemyVisualRulesDefinition.Unrestricted();

            var weights = rules.BuildThemeWeightMap();

            return weights.Count == 0
                ? EnemyVisualRulesDefinition.Unrestricted()
                : EnemyVisualRulesDefinition.FromWeightMap(weights);
        }
    }
}