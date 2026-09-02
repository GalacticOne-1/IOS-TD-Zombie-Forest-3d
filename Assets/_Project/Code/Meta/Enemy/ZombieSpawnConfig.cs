using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Дополнение к LocationConfig — описывает волны зомби для локации.
    /// Добавить поле в существующий LocationConfig ScriptableObject.
    /// </summary>
    [System.Serializable]
    public sealed class ZombieSpawnConfig
    {
        [Tooltip("Точки спавна зомби в сцене (Transform-ы из иерархии сцены)")]
        public List<Transform> SpawnPoints = new();

        [Tooltip("Максимум зомби одновременно на сцене")]
        public int MaxAliveCount = 20;

        [Tooltip("Волны зомби")]
        public List<ZombieWaveConfig> Waves = new();
    }

    [System.Serializable]
    public sealed class ZombieWaveConfig
    {
        [Tooltip("Через сколько секунд после старта рейда запустить волну")]
        public float TriggerTime;

        [Tooltip("Записи: сколько и каких зомби спавнить в этой волне")]
        public List<ZombieWaveEntry> Entries = new();
    }

    [System.Serializable]
    public sealed class ZombieWaveEntry
    {
        public EnemyArchetypeConfig Variant;
        public int Count;
    }
}