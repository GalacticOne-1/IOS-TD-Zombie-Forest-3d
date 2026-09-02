
using Galactic1.Code.WorldMap.Definitions;

namespace Galactic1.Code.Gameplay.Enemies.Variants
{
    /// <summary>
    /// Контекст запроса к EnemyVariantResolver.
    /// RequestedVariantId удалён — новая архитектура не поддерживает
    /// явный выбор скина по ID (тема + случайный индекс).
    /// </summary>
    public sealed class EnemyVariantResolveContext
    {
        public EnemyVisualRulesDefinition VisualRules { get; set; }

        public static EnemyVariantResolveContext From(LocationDefinition locationDef) =>
            new() { VisualRules = locationDef?.EnemyVisualRules };

        public static EnemyVariantResolveContext Unrestricted() =>
            new() { VisualRules = EnemyVisualRulesDefinition.Unrestricted() };
    }
}