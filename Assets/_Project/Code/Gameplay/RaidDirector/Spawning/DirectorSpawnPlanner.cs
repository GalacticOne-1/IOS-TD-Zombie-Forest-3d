using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using UnityEngine;

namespace Galactic1.Code.Gameplay.RaidDirector
{
    /// <summary>
    /// Формирует список EnemySpawnRequest для группы Director.
    ///
    /// v2: Planner НЕ вычисляет groupSize — получает его готовым из RaidDirectorRuntime.
    /// Единственная ответственность: взять позиции + EnemyId → список запросов.
    /// </summary>
    public sealed class DirectorSpawnPlanner
    {
        /// <summary>
        /// Сформировать список запросов спавна.
        /// </summary>
        /// <param name="groupSize">Размер группы — вычислен в RaidDirectorRuntime.</param>
        /// <param name="enemyId">Архетип врага.</param>
        /// <param name="positions">Позиции — от DirectorSpawnResolver, уже проверены.</param>
        public List<EnemySpawnRequest> Plan(
            int groupSize,
            EnemyId enemyId,
            List<Vector3> positions)
        {
            var requests = new List<EnemySpawnRequest>(groupSize);

            int count = Mathf.Min(groupSize, positions.Count);

            for (int i = 0; i < count; i++)
            {
                requests.Add(new EnemySpawnRequest(
                    enemyId: enemyId,
                    position: positions[i],
                    variantId: string.Empty,
                    modifierIds: null,
                    waveIndex: -1, // -1 = не волна
                    source: SpawnSource.Director // ← тег Director
                ));
            }

            return requests;
        }
    }
}