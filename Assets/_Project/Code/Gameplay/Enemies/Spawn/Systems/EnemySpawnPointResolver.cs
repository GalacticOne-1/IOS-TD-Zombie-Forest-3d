
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Spawning.Positioning
{
    /// <summary>
    /// Резолвер точки спавна: конвертирует описание региона в конкретные координаты.
    ///
    /// WaveSystem задаёт регионы/точки входа, а не точные координаты.
    /// Этот класс выбирает финальную позицию внутри региона.
    ///
    /// Поддерживает:
    ///   — точную позицию (Position из EnemySpawnRequest)
    ///   — случайную точку вокруг центра
    ///   — navmesh sampling (хук для будущего)
    ///   — точки входа из локации (хук для будущего)
    ///
    /// Хуки для будущих систем:
    ///   — NavMesh.SamplePosition для размещения на проходимой поверхности
    ///   — SpawnZone из локационного конфига
    ///   — Offscreen spawning (вне поля зрения игрока)
    /// </summary>
    public sealed class EnemySpawnPointResolver
    {
        /// <summary>Радиус рандомизации вокруг базовой точки спавна.</summary>
        public float RandomRadius { get; set; } = 2f;

        /// <summary>
        /// Резолвит финальную позицию спавна из запроса.
        ///
        /// Сейчас: случайное смещение в пределах RandomRadius.
        /// Будущее: NavMesh sampling, offscreen spawning.
        /// </summary>
        public Vector3 Resolve(EnemySpawnRequest request)
        {
            var basePos = request.Position;

            // Случайное смещение чтобы враги не спавнились ровно в одну точку
            var offset = new Vector3(
                Random.Range(-RandomRadius, RandomRadius),
                0f,
                Random.Range(-RandomRadius, RandomRadius));

            return basePos + offset;

            // ХУК: NavMesh sampling (раскомментировать когда нужен navmesh)
            // if (NavMesh.SamplePosition(basePos + offset, out var hit, RandomRadius * 2f, NavMesh.AllAreas))
            //     return hit.position;
            // return basePos;
        }
    }
}