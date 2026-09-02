
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Spawning
{
    /// <summary>
    /// Система очереди и темпа спавна врагов.
    ///
    /// ОТВЕТСТВЕННОСТЬ:
    ///   — хранение очереди EnemySpawnRequest
    ///   — тикинг и бюджет спавна (макс N врагов за тик)
    ///   — опциональные задержки между спавнами
    ///
    /// НЕ ДЕЛАЕТ:
    ///   — логику волн (это WaveSystem)
    ///   — логику спавна (это EnemySpawnPipeline)
    ///
    /// Использование:
    ///   spawnSystem.Enqueue(request);       // добавить в очередь
    ///   spawnSystem.Tick(dt);               // вызывать каждый кадр из RaidRuntime
    /// </summary>
    public sealed class EnemySpawnSystem
    {
        private readonly Queue<EnemySpawnRequest> _queue = new();
        private readonly EnemySpawnPipeline _pipeline;

        /// <summary>Максимум спавнов за один тик (ограничение бюджета).</summary>
        public int MaxSpawnsPerTick { get; set; } = 3;

        /// <summary>
        /// Минимальная задержка между спавнами отдельных врагов, секунды.
        /// 0 = без задержки.
        /// </summary>
        public float SpawnInterval { get; set; } = 0f;

        private float _spawnTimer;

        public EnemySpawnSystem(EnemySpawnPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        /// <summary>
        /// Добавляет запрос спавна в очередь.
        /// Единственный публичный вход — WaveSystem, DEV_polygon, любой внешний источник.
        /// </summary>
        public void Enqueue(EnemySpawnRequest request)
        {
            _queue.Enqueue(request);
#if UNITY_EDITOR
            Debug.Log($"[EnemySpawnSystem] В очереди: {_queue.Count} | Добавлен: {request}");
#endif
        }

        /// <summary>
        /// Вызывается каждый игровой тик (из SUB_RaidActiveState или RaidRuntime.Tick).
        /// Обрабатывает очередь с учётом бюджета и задержки.
        /// </summary>
        public void Tick(float dt)
        {
            if (_queue.Count == 0) return;

            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            int spawned = 0;
            while (_queue.Count > 0 && spawned < MaxSpawnsPerTick)
            {
                var request = _queue.Dequeue();
                _pipeline.Spawn(request);
                spawned++;
            }

            // Сбрасываем таймер после спавна
            if (spawned > 0)
                _spawnTimer = SpawnInterval;
        }

        /// <summary>Возвращает количество запросов в очереди.</summary>
        public int PendingCount => _queue.Count;

        /// <summary>Очищает очередь (например при выходе из рейда).</summary>
        public void Clear() => _queue.Clear();
    }
}