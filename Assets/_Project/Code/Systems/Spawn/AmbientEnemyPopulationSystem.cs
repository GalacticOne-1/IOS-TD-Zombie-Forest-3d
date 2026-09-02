
using Galactic1.Code.Gameplay.Enemies.Authoring;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Configs.Enemies;
using UnityEngine;

namespace Galactic1.Code.Systems.Enemies
{
    /// <summary>
    /// Спавнит ambient-группы врагов при старте рейда.
    ///
    /// Читает AmbientSpawnPoint[] из LocationContext —
    /// сцена является единственным источником пространственных данных.
    ///
    /// НЕ создаёт GameObject, EnemyInstance, не трогает сцену.
    /// </summary>
    public sealed class AmbientEnemyPopulationSystem
    {
        private readonly EnemySpawnSystem _spawnSystem;
        private readonly EnemySpawnPoint[] _spawnPoints;

        public AmbientEnemyPopulationSystem(
            EnemySpawnSystem spawnSystem,
            EnemySpawnPoint[] spawnPoints)
        {
            _spawnSystem = spawnSystem;
            _spawnPoints = spawnPoints ?? System.Array.Empty<EnemySpawnPoint>();
        }

        public void Initialize()
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogWarning("[AmbientEnemyPopulationSystem] Нет AmbientSpawnPoints — враги не заспавнены.");
                return;
            }

            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint == null)
                {
                    Debug.LogWarning("[AmbientEnemyPopulationSystem] SpawnPoint == null — пропущена.");
                    continue;
                }

                if (spawnPoint.Group == null)
                {
                    Debug.LogWarning($"[AmbientEnemyPopulationSystem] '{spawnPoint.name}': Group == null — пропущена.");
                    continue;
                }

                SpawnGroup(spawnPoint);
            }
        }

        private void SpawnGroup(EnemySpawnPoint spawnPoint)
        {
            var group = spawnPoint.Group;
            var origin = spawnPoint.transform.position;
            var wanderRadius = spawnPoint.WanderRadius;
            var total = CountTotalEnemies(group);
            var spawnIndex = 0;

            foreach (var entry in group.Enemies)
            {
                if (entry.Enemy == null)
                {
                    Debug.LogWarning(
                        $"[AmbientEnemyPopulationSystem] Группа '{group.GroupId}': Enemy == null — пропущена.");
                    continue;
                }

                for (int i = 0; i < entry.Count; i++)
                {
                    var position = ResolvePosition(origin, wanderRadius, spawnIndex, total);
                    spawnIndex++;

                    _spawnSystem.Enqueue(new EnemySpawnRequest(
                        entry.Enemy.Id,
                        position,
                        "",
                        null,
                        0,
                        SpawnSource.Static));
                    
                    _spawnSystem.Tick(0f);
                }
            }

#if UNITY_EDITOR
            DLog.Alert($"[AmbientEnemyPopulationSystem] '{group.GroupId}' — {total} врагов в очереди.");
#endif
        }

        private static int CountTotalEnemies(EnemyGroupConfig group)
        {
            int total = 0;
            foreach (var entry in group.Enemies)
                total += entry.Count;
            return total;
        }

        private static Vector3 ResolvePosition(Vector3 origin, float wanderRadius, int index, int total)
        {
            if (index == 0 || total <= 1)
                return origin;

            float angle = (360f / total) * index * Mathf.Deg2Rad;
            float spread = Mathf.Max(wanderRadius * 0.4f, 1f);

            return origin + new Vector3(
                Mathf.Cos(angle) * spread,
                0f,
                Mathf.Sin(angle) * spread);
        }
    }
}