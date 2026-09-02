using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Configs.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    [Serializable]
    public sealed class WaveSpawnInstruction
    {
        [Tooltip("Id точки спавна (WaveSpawnPoint.SpawnId).")]
        public WaveSpawnId SpawnPointId;
        
        [Space(20)]
        [Tooltip("Состав группы — переиспользуется существующий конфиг.")]
        public EnemyGroupConfig Group;


        [Tooltip("Задержка перед началом спавна этой группы, сек. Отсчитывается с момента, когда инструкция разблокирована (см. WaitPreviousInstruction).")]
        public float Delay = 0f;

        [Tooltip("Интервал между отдельными врагами внутри группы, сек.")]
        public float Interval = 0.5f;

        [Tooltip("Если true — инструкция не начинает отсчёт Delay, пока не завершится предыдущая инструкция в списке этой волны. Позволяет строить последовательности (Group A → wait → Group B) без создания новой волны.")]
        public bool WaitPreviousInstruction = false;
    }
}