using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Authoring
{
    /// <summary>
    /// Точка входа для волн зомби (Camp Defense).
    ///
    /// НЕ является заменой EnemySpawnPoint — тот описывает статичную
    /// ambient-группу (кто + где). WaveSpawnPoint описывает ТОЛЬКО "где":
    /// состав волны задаётся отдельно, в WaveSpawnInstruction, через
    /// уже существующий EnemyGroupConfig.
    /// </summary>
    public sealed class WaveSpawnPoint : MonoBehaviour
    {
        [Tooltip("Идентификатор точки, на который ссылаются WaveSpawnInstruction.")]
        public WaveSpawnId SpawnId;

        [Tooltip("Радиус разброса точек спавна вокруг центра (используется поверх стандартной рандомизации).")]
        public float SpawnRadius = 2f;

        public bool Enabled = true;

        public Vector3 Position => transform.position;
    }
}