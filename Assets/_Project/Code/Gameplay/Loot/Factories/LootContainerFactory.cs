using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Scene;

namespace Galactic1.RaidLoot.Services
{
    public sealed class LootContainerFactory
    {
        private readonly LootContainerRepository _repository;
        private readonly Dictionary<LootSpawnPoint, LootContainerRuntime> _bindings = new();
        

        public LootContainerFactory(LootContainerRepository repository)
            => _repository = repository;

        public void BuildAll(IEnumerable<LootSpawnPoint> spawnPoints)
        {
            foreach (var point in spawnPoints)
            {
                if (point?.Config == null) 
                    continue;
                
                var definition = BuildDefinition(point.Config);
                var runtime = new LootContainerRuntime(definition);
                
                _repository.Register(runtime);
                _bindings[point] = runtime;
            }
        }
        
        public bool TryGetRuntime(
            LootSpawnPoint point,
            out LootContainerRuntime runtime)
        {
            return _bindings.TryGetValue(point, out runtime);
        }

        private static LootContainerDefinition BuildDefinition(LootContainerDefinitionConfig config)
            => new LootContainerDefinition(
                config.Id,
                config.ContainerType,
                BuildTableDefinition(config.LootTableConfig),
                config.ContainerTier,
                config.OpenRadius,
                config.OpenTimerDelay,
                config.VisualId);

        private static LootTableDefinition BuildTableDefinition(LootTableConfig config)
            => new LootTableDefinition(
                config.Id,
                config.Slots,
                config.GuaranteedEntries);
    }
}