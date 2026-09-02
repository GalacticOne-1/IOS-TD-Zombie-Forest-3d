
using System.Collections.Generic;
using Galactic1.Game.Meta.Enemy.Modifiers;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Фабрика data-driven модификаторов.
    ///
    /// Конвертирует authoring EnemyModifierConfig (SO) в IEnemyModifier реализацию.
    ///
    /// Слой:
    ///   EnemyModifierConfig (SO, authoring)
    ///     ↓  EnemyModifierFactory.CreateFromConfig()
    ///   EnemyModifierDefinition (runtime, immutable)
    ///     ↓  DataDrivenModifier.Apply(EnemyModifierContext)
    ///   EnemyMutationContext (mutable, pipeline)
    ///
    /// Используется EnemyModifierDatabase.RegisterFromConfig().
    /// </summary>
    public sealed class EnemyModifierFactory
    {
        public IEnemyModifier CreateFromConfig(EnemyModifierConfig config)
        {
            var definition = BuildDefinition(config);
            return new DataDrivenModifier(definition);
        }

        private EnemyModifierDefinition BuildDefinition(EnemyModifierConfig config)
        {
            var statMultipliers = new List<EnemyModifierDefinition.StatMultiplierEntry>(
                config.StatMultipliers.Count);

            foreach (var entry in config.StatMultipliers)
                statMultipliers.Add(new EnemyModifierDefinition.StatMultiplierEntry(
                    entry.StatId, entry.Multiplier));

            return new EnemyModifierDefinition(
                config.ModifierId,
                config.DisplayName,
                statMultipliers,
                config.WalkSpeedMultiplier,
                config.RunSpeedMultiplier,
                config.PrefabIdOverride,
                config.AnimatorOverride,
                config.SetsEliteFlag,
                config.ThreatMultiplierBonus);
        }
    }
}