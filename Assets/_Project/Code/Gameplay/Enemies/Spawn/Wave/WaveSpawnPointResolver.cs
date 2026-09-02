using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Authoring;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>
    /// Резолвит WaveSpawnPoint по SpawnId.
    ///
    /// НЕ путать с Spawning.Positioning.EnemySpawnPointResolver — тот
    /// работает ПОЗЖЕ, внутри EnemySpawnPipeline, и рандомизирует
    /// финальную позицию для ЛЮБОГО источника спавна. Этот резолвер решает
    /// более раннюю задачу: превращает строковый Id точки входа волны
    /// в её базовую мировую позицию.
    /// </summary>
    public sealed class WaveSpawnPointResolver
    {
        private readonly Dictionary<WaveSpawnId, WaveSpawnPoint> _points = new();

        public WaveSpawnPointResolver(IEnumerable<WaveSpawnPoint> points)
        {
            foreach (var point in points)
            {
                if (point == null || !point.Enabled) continue;

                if (!_points.TryAdd(point.SpawnId, point))
                    Debug.LogWarning(
                        $"[WaveSpawnPointResolver] Дублирующийся SpawnId='{point.SpawnId}'.");
            }
        }

        public Vector3 Resolve(WaveSpawnId spawnId)
        {
            if (_points.TryGetValue(spawnId, out var point))
                return point.Position;

            Debug.LogError(
                $"[WaveSpawnPointResolver] WaveSpawnPoint с SpawnId='{spawnId}' не найден.");
            return Vector3.zero;
        }
    }
}