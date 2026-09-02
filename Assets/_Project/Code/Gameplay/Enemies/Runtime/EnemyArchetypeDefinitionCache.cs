
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Factories;
using Galactic1.Configs;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Enemies.Definitions
{
    public sealed class EnemyArchetypeDefinitionCache
    {
        private readonly Dictionary<EnemyId, EnemyArchetypeDefinition> _cache = new();
        private readonly EnemyArchetypeDefinitionBuilder _builder;
        private readonly ConfigProvider _configProvider;

        public EnemyArchetypeDefinitionCache(
            EnemyArchetypeDefinitionBuilder builder,
            ConfigProvider configProvider)
        {
            _builder = builder;
            _configProvider = configProvider;
        }

        public EnemyArchetypeDefinition Get(EnemyId enemyId)
        {
            if (_cache.TryGetValue(enemyId, out var cached))
                return cached;

            var db = _configProvider.Get<ZombieVariantDatabase>();
            var config = db?.GetById(enemyId);
            if (config == null) return null;

            var definition = _builder.Build(config);
            _cache[enemyId] = definition;
            return definition;
        }

        public void Clear() => _cache.Clear();
    }
}