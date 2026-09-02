
using Galactic1.RaidLoot.Scene;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Systems
{
    /// <summary>
    /// Аналог AmbientEnemyPopulationSystem для лута.
    ///
    /// Читает LootSpawnPoint[] из LocationContext и строит
    /// LootContainerRuntime через LootContainerFactory.
    ///
    /// Не знает про View, EventBus, буфер.
    /// </summary>
    public sealed class LootPopulationSystem
    {
        private readonly LootContainerFactory _factory;
        private readonly LootSpawnPoint[] _spawnPoints;

        public LootPopulationSystem(
            LootContainerFactory factory,
            LootSpawnPoint[] spawnPoints)
        {
            _factory = factory;
            _spawnPoints = spawnPoints ?? System.Array.Empty<LootSpawnPoint>();
        }

        public void Initialize()
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogWarning("[LootPopulationSystem] Нет LootSpawnPoints — контейнеры не созданы.");
                return;
            }

            _factory.BuildAll(_spawnPoints);

            Debug.Log($"[LootPopulationSystem] Инициализировано {_spawnPoints.Length} контейнеров.");
        }
    }
}