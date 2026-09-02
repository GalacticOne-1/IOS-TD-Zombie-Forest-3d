using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Spawning.Requests
{
    /// <summary>
    /// Иммутабельная команда спавна одного врага.
    ///
    /// Изменение v2: добавлено поле Source (SpawnSource).
    /// Все существующие вызовы остаются совместимыми — Source имеет дефолтное значение Static.
    /// </summary>
    public readonly struct EnemySpawnRequest
    {
        public readonly EnemyId EnemyId;
        public readonly Vector3 Position;
        public readonly string VariantId;
        public readonly IReadOnlyList<string> ModifierIds;
        public readonly int WaveIndex;

        /// <summary>Кто инициировал спавн. По умолчанию Static — обратная совместимость.</summary>
        public readonly SpawnSource Source;

        public EnemySpawnRequest(
            EnemyId enemyId,
            Vector3 position,
            string variantId = "",
            IReadOnlyList<string> modifierIds = null,
            int waveIndex = 0,
            SpawnSource source = SpawnSource.Static)
        {
            EnemyId = enemyId;
            Position = position;
            VariantId = variantId ?? string.Empty;
            ModifierIds = modifierIds ?? System.Array.Empty<string>();
            WaveIndex = waveIndex;
            Source = source;
        }

        public override string ToString() =>
            $"[SpawnRequest] EnemyId={EnemyId} Wave={WaveIndex} " +
            $"Source={Source} Variant={VariantId} Pos={Position}";
    }
}