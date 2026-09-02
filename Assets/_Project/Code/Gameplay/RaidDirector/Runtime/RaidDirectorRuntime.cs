using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Gameplay.Noise;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.RaidDirector
{
    /// <summary>
    /// Raid Director v2.
    ///
    /// Изменения относительно v1:
    ///   — больше не реализует INoiseListener
    ///   — подписывается на NoiseSystem.OnNoiseEmitted
    ///   — не содержит NoiseWeight / TranslateNoiseType (перенесено в ThreatModel)
    ///   — не содержит _pendingDirectorSpawns / HashSet _directorSpawnIds
    ///   — трекинг бюджета через runtime.SpawnSource == SpawnSource.Director
    ///   — TrySpawn() разбит на CanSpawn / ResolveGroupSize / ResolvePositions /
    ///     BuildRequests / EnqueueRequests / CommitSpawn
    ///   — после спавна вызывает ThreatModel.ProcessSpawnCommitted()
    ///   — проверяет GlobalAliveEnemyLimit перед спавном
    /// </summary>
    public sealed class RaidDirectorRuntime
    {
        // ── Зависимости ────────────────────────────────────────────────
        private readonly DirectorConfig _config;
        private readonly DirectorThreatModel _threat;
        private readonly DirectorSpawnBudget _budget;
        private readonly DirectorSpawnPlanner _planner;
        private readonly DirectorSpawnResolver _resolver;
        private readonly EnemySpawnSystem _spawnSystem;
        private readonly RaidEnemyRegistry _enemyRegistry;
        private readonly NoiseSystem _noiseSystem;

        // ── Состояние ──────────────────────────────────────────────────
        private float _spawnCooldownRemaining;
        private EnemyId _defaultEnemyId;
        private Transform _playerTransform;

        // ── Публичное состояние (дебаг / UI) ──────────────────────────
        public float Threat => _threat.Threat;
        public DirectorState State => _threat.State;
        public int AliveFromDirector => _budget.CurrentAlive;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public RaidDirectorRuntime(
            DirectorConfig config,
            EnemySpawnSystem spawnSystem,
            RaidEnemyRegistry enemyRegistry,
            NoiseSystem noiseSystem,
            DirectorSpawnResolver resolver)
        {
            _config = config;
            _spawnSystem = spawnSystem;
            _enemyRegistry = enemyRegistry;
            _noiseSystem = noiseSystem;
            _resolver = resolver;

            _threat = new DirectorThreatModel(config);
            _budget = new DirectorSpawnBudget(config);
            _planner = new DirectorSpawnPlanner();
        }

        // ─────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Подключить Director к источникам событий.
        /// Вызвать из SUB_RaidStartState.Enter().
        /// </summary>
        public void Initialize(EnemyId defaultEnemyId, Transform playerTransform)
        {
            _defaultEnemyId = defaultEnemyId;
            _playerTransform = playerTransform;

            _noiseSystem.OnNoiseEmitted += OnNoiseEmitted;
            _enemyRegistry.OnRegistered += OnEnemyRegistered;
        }

        /// <summary>
        /// Отключить Director. Вызвать из Exit рейда или Cleanup.
        /// </summary>
        public void Dispose()
        {
            _noiseSystem.OnNoiseEmitted -= OnNoiseEmitted;
            _enemyRegistry.OnRegistered -= OnEnemyRegistered;
        }

        // ─────────────────────────────────────────────────────────────
        // Tick
        // ─────────────────────────────────────────────────────────────

        /// <summary>Вызывать из SUB_RaidActiveState.Update(dt).</summary>
        public void Tick(float dt)
        {
            _threat.Decay(dt);

            if (_spawnCooldownRemaining > 0f)
                _spawnCooldownRemaining -= dt;

            TrySpawn();

#if UNITY_EDITOR
            DebugLog();
#endif
        }

        // ─────────────────────────────────────────────────────────────
        // Public event entry points
        // ─────────────────────────────────────────────────────────────

        /// <summary>Игрок получил урон.</summary>
        public void OnPlayerDamaged() => _threat.ProcessPlayerDamaged();

        // ─────────────────────────────────────────────────────────────
        // Event handlers (private)
        // ─────────────────────────────────────────────────────────────

        private void OnNoiseEmitted(NoiseEvent evt)
        {
            _threat.ProcessNoise(evt);
        }

        private void OnEnemyRegistered(EnemyRuntime runtime)
        {
            // Подписываемся на смерть каждого врага независимо от источника.
            // HandleEnemyDeath сам проверит SpawnSource.
            runtime.OnDeath += () => HandleEnemyDeath(runtime);
        }

        private void HandleEnemyDeath(EnemyRuntime runtime)
        {
            _threat.ProcessEnemyKilled();

            if (runtime.SpawnSource == SpawnSource.Director)
                _budget.OnDirectorEnemyKilled();
        }

        // ─────────────────────────────────────────────────────────────
        // Spawn pipeline (разбит на методы)
        // ─────────────────────────────────────────────────────────────

        private void TrySpawn()
        {
            if (!CanSpawn()) return;

            int groupSize = ResolveGroupSize();
            if (groupSize <= 0) return;

            var positions = ResolvePositions(groupSize);
            if (positions.Count == 0) return;

            var requests = BuildRequests(groupSize, positions);
            EnqueueRequests(requests);
            CommitSpawn(requests.Count);
        }

        private bool CanSpawn()
        {
            if (_threat.Threat < _config.MinThreatToSpawn) return false;
            if (_threat.State == DirectorState.Calm) return false;
            if (!_budget.HasBudget) return false;
            if (_spawnCooldownRemaining > 0f) return false;
            if (_defaultEnemyId == null) return false;
            if (_playerTransform == null) return false;

            // Глобальный лимит живых врагов на карте
            if (_enemyRegistry.AliveCount >= _config.GlobalAliveEnemyLimit) return false;

            return true;
        }

        private int ResolveGroupSize()
        {
            int sizeByState = _threat.State switch
            {
                DirectorState.Searching => _config.GroupSizeSearching,
                DirectorState.Pressure => _config.GroupSizePressure,
                DirectorState.Hunting => _config.GroupSizeHunting,
                _ => 0
            };

            // Ограничиваем бюджетом Director, глобальным лимитом и MaxGroupSize
            int globalHeadroom = _config.GlobalAliveEnemyLimit - _enemyRegistry.AliveCount;

            return Mathf.Min(
                sizeByState,
                _budget.Remaining,
                globalHeadroom,
                _config.MaxGroupSize);
        }

        private List<Vector3> ResolvePositions(int groupSize)
        {
            var positions = _resolver.Resolve(_playerTransform.position, groupSize);

#if UNITY_EDITOR
            if (positions.Count == 0)
                Debug.LogWarning("[RaidDirector] Не удалось найти валидные позиции спавна.");
#endif

            return positions;
        }

        private List<EnemySpawnRequest> BuildRequests(int groupSize, List<Vector3> positions)
        {
            return _planner.Plan(groupSize, _defaultEnemyId, positions);
        }

        private void EnqueueRequests(List<EnemySpawnRequest> requests)
        {
            foreach (var req in requests)
                _spawnSystem.Enqueue(req);
        }

        private void CommitSpawn(int count)
        {
            _budget.OnSpawned(count);
            _spawnCooldownRemaining = _config.SpawnCooldown;
            _threat.ProcessSpawnCommitted();

#if UNITY_EDITOR
            Debug.Log(
                $"[RaidDirector] Спавн группы {count} | " +
                $"State={_threat.State} | Threat={_threat.Threat:F1} | " +
                $"DirectorAlive={_budget.CurrentAlive}/{_config.MaxAliveFromDirector} | " +
                $"GlobalAlive={_enemyRegistry.AliveCount}/{_config.GlobalAliveEnemyLimit}");
#endif
        }

        // ─────────────────────────────────────────────────────────────
        // Debug
        // ─────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private float _debugLogTimer;

        private void DebugLog()
        {
            _debugLogTimer -= Time.deltaTime;
            if (_debugLogTimer > 0f) return;
            _debugLogTimer = 3f;

            DLog.Alert(
                $"[RaidDirector] State={_threat.State} | Threat={_threat.Threat:F1} | " +
                $"DirectorAlive={_budget.CurrentAlive}/{_config.MaxAliveFromDirector} | " +
                $"GlobalAlive={_enemyRegistry.AliveCount}/{_config.GlobalAliveEnemyLimit} | " +
                $"Cooldown={_spawnCooldownRemaining:F1}s",
                EDlogColor.YELLOW);
        }
#endif
    }
}