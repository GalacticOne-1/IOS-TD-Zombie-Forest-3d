
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Variants;

namespace Galactic1.Code.WorldMap.Definitions
{
    public sealed class LocationDefinition
    {
        public LocationId Id { get; }
        public EnemyVisualRulesDefinition EnemyVisualRules { get; }

        public LocationDefinition(LocationId id, EnemyVisualRulesDefinition enemyVisualRules)
        {
            Id = id;
            EnemyVisualRules = enemyVisualRules;
        }
    }
}