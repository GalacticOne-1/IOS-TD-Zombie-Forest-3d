
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Factories
{
    /// <summary>
    /// Чистая фабрика: EnemyRuntimeDefinition + Vector3 → EnemyRuntime.
    ///
    /// ЕДИНСТВЕННАЯ ОТВЕТСТВЕННОСТЬ: конструирование EnemyRuntime.
    ///
    /// НЕ ДЕЛАЕТ:
    ///   — построение Definition (это EnemyRuntimeDefinitionBuilder)
    ///   — резолюцию вариантов (это EnemyVariantResolver + EnemyPresentationFactory)
    ///   — применение модификаторов (это EnemyModifierPipeline)
    ///
    /// Вызывается: только из EnemySpawnPipeline, после того как
    ///   Definition полностью собрана и иммутабельна.
    /// </summary>
    public sealed class EnemyRuntimeFactory
    {
        /// <summary>
        /// Создаёт EnemyRuntime из иммутабельного определения и позиции спавна.
        /// </summary>
        public EnemyRuntime Create(EnemyRuntimeDefinition definition, Vector3 spawnPosition, SpawnSource spawnSource)
        {
            return new EnemyRuntime(definition, spawnPosition, 0, spawnSource);
        }
    }
}