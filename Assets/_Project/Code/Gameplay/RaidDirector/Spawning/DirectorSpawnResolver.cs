using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Galactic1.Code.Gameplay.RaidDirector
{
    /// <summary>
    /// Ищет валидные позиции для спавна группы Director:
    ///   — в кольце [MinSpawnDistance .. MaxSpawnDistance] от игрока
    ///   — вне viewport камеры
    ///   — на NavMesh
    /// </summary>
    public sealed class DirectorSpawnResolver
    {
        private readonly DirectorConfig _config;
        private readonly Camera _camera;

        public DirectorSpawnResolver(DirectorConfig config, Camera camera)
        {
            _config = config;
            _camera = camera;
        }

        public List<Vector3> Resolve(Vector3 playerPosition, int groupSize)
        {
            var result = new List<Vector3>(groupSize);
            int maxAttempts = _config.SpawnPositionAttempts * groupSize;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (result.Count >= groupSize) break;

                var candidate = GenerateCandidate(playerPosition);

                if (!IsValidPosition(candidate, playerPosition))
                    continue;

                var offset = new Vector3(
                    Random.Range(-2f, 2f),
                    0f,
                    Random.Range(-2f, 2f));

                result.Add(candidate + offset);
            }

            return result;
        }

        private Vector3 GenerateCandidate(Vector3 origin)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(_config.MinSpawnDistance, _config.MaxSpawnDistance);

            var candidate = origin + new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance);

            if (NavMesh.SamplePosition(candidate, out var hit, 5f, NavMesh.AllAreas))
                return hit.position;

            return candidate;
        }

        private bool IsValidPosition(Vector3 candidate, Vector3 playerPosition)
        {
            float dist = Vector3.Distance(candidate, playerPosition);
            if (dist < _config.MinSpawnDistance || dist > _config.MaxSpawnDistance)
                return false;

            if (_camera != null)
            {
                var vp = _camera.WorldToViewportPoint(candidate);
                bool onScreen = vp.z > 0f
                                && vp.x is >= 0f and <= 1f
                                && vp.y is >= 0f and <= 1f;
                if (onScreen) return false;
            }

            return true;
        }
    }
}