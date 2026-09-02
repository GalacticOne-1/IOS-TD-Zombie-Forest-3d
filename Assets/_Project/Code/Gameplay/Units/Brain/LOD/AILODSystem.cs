using System;
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Core;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.AI.LOD
{
    /// <summary>
    /// Централизованная система AI LOD ("Sleeping Zombies").
    ///
    /// АРХИТЕКТУРНОЕ ПРАВИЛО:
    /// Это ЕДИНСТВЕННОЕ место, где считаются дистанции и принимаются
    /// решения об уровне симуляции. EnemyInstance никогда не решает
    /// сам за себя — только исполняет SetSimulationLevel(level).
    ///
    /// Живёт как дочерняя система RaidRuntime (создаётся/уничтожается
    /// вместе с рейдом, как CombatRuntime и RaidDirectorRuntime).
    ///
    /// Источники данных:
    ///   EnemyRepository   — scene-слой, даёт EnemyInstance для вызова API
    ///   RaidEnemyRegistry — runtime-слой, даёт события регистрации/спавна
    ///   EnemyBlackboard   — per-unit AI память (уже существует, только читаем)
    ///
    /// Вызов дистанций к отряду инкапсулирован за Func&lt;Vector3&gt;,
    /// чтобы AILODSystem не знал КАК считается центр отряда
    /// (FormationCenterDriver / camera group / etc — решает вызывающий код).
    /// </summary>
    public sealed class AILODSystem
    {
        private readonly AILODConfig _config;
        private readonly EnemyRepository _enemyRepository;
        private readonly RaidEnemyRegistry _enemyRegistry;
        private Func<Vector3> _squadCenterProvider;

        // Трекинг метаданных спавна по runtime.Id — нужен только для
        // Director grace period. Заполняется через события регистрации,
        // а не пересчитывается на каждую итерацию.
        private readonly Dictionary<string, float> _spawnTime = new();
        private readonly Dictionary<string, SpawnSource> _spawnSource = new();

        private readonly float _fullRadiusSqr;
        private readonly float _lowRadiusSqr;

        private float _evalTimer;
        private bool initialized;

        private Vector3 squadCenter;

        public AILODSystem(
            AILODConfig config,
            EnemyRepository enemyRepository,
            RaidEnemyRegistry enemyRegistry)
        {
            initialized = false;
            _config = config;
            _enemyRepository = enemyRepository;
            _enemyRegistry = enemyRegistry;
            

            _fullRadiusSqr = config.FullSimulationRadius * config.FullSimulationRadius;
            _lowRadiusSqr = config.LowSimulationRadius * config.LowSimulationRadius;

            _enemyRegistry.OnRegistered += HandleRegistered;
            _enemyRegistry.OnUnregistered += HandleUnregistered;
        }

        public void Initialize(Func<Vector3> squadCenterProvider)
        {
            _squadCenterProvider = squadCenterProvider;
            initialized = true;
        }

        /// <summary>
        /// Начальный проход когда все юниты в сцене готовы
        /// </summary>
        public void Entry()
        {
            squadCenter = ServiceLocator.Current.Get<CameraController>().FocusPosition;
            Evaluate();
        }

        /// <summary>Вызывать вместе с disposal остальных raid-систем (CombatRuntime.Dispose и т.п.).</summary>
        public void Dispose()
        {
            _enemyRegistry.OnRegistered -= HandleRegistered;
            _enemyRegistry.OnUnregistered -= HandleUnregistered;

            _spawnTime.Clear();
            _spawnSource.Clear();
        }

        // ── Registration ────────────────────────────────────────────────
        // Регистрируемся на RaidEnemyRegistry, а не на EnemyRepository:
        // нам нужен момент появления EnemyRuntime (для timestamp'а спавна),
        // который происходит раньше и надёжнее, чем сборка сцен-объекта.

        private void HandleRegistered(EnemyRuntime runtime)
        {
            _spawnTime[runtime.Id] = Time.time;
            _spawnSource[runtime.Id] = runtime.SpawnSource;
        }

        private void HandleUnregistered(string id)
        {
            _spawnTime.Remove(id);
            _spawnSource.Remove(id);
        }

        // ── Tick ─────────────────────────────────────────────────────────

        /// <summary>Вызывается из RaidRuntime.Tick(dt).</summary>
        public void Tick(float dt)
        {
            if (!initialized)
                return;
            
            _evalTimer += dt;
            if (_evalTimer < _config.EvaluationInterval) return;
            _evalTimer = 0f;

            
            squadCenter = _squadCenterProvider != null
                ? _squadCenterProvider()
                : Vector3.zero;
            
            Evaluate();
        }

        private void Evaluate()
        {
            
            float now = Time.time;

            // EnemyRepository.ActiveEnemies — filtered view поверх
            // UnitSceneRepository, уже существует, ничего нового не создаём.
            foreach (var enemy in _enemyRepository.ActiveEnemies)
            {
                var runtime = enemy.EnemyAdapter?.Runtime;
                if (runtime == null) continue; // сцена ещё не забиндена — пропускаем цикл

                var level = Decide(enemy, (EnemyRuntime)runtime, squadCenter, now);
                enemy.SetSimulationLevel(level); // единственная точка исполнения
            }
        }

        // ── Decision ─────────────────────────────────────────────────────

        private SimulationLevel Decide(
            EnemyInstance enemy,
            EnemyRuntime runtime,
            Vector3 squadCenter,
            float now)
        {
            // 1) Director grace period — свежеспавненный директором враг
            //    не должен засыпать раньше, чем успеет вступить в контакт.
            if (_spawnSource.TryGetValue(runtime.Id, out var source) &&
                source == SpawnSource.Director)
            {
                float spawnedAt = _spawnTime.TryGetValue(runtime.Id, out var t) ? t : now;
                if (now - spawnedAt < _config.DirectorSpawnGracePeriod)
                    return SimulationLevel.Full;
            }

            // 2) Игровое состояние важнее дистанции. Если враг в бою,
            //    услышал шум или держит pack-слот — он не может уснуть,
            //    даже если формально далеко от центра отряда.
            if (IsGameplayCritical(enemy))
                return SimulationLevel.Full;

            // 3) Чистая дистанция — fallback для всех остальных случаев.
            float sqrDist = (enemy.transform.position - squadCenter).sqrMagnitude;

            if (sqrDist <= _fullRadiusSqr) return SimulationLevel.Full;
            if (sqrDist <= _lowRadiusSqr) return SimulationLevel.Low;
            return SimulationLevel.Sleeping;
        }

        private static bool IsGameplayCritical(EnemyInstance enemy)
        {
            // Переиспользуем существующий EnemyBlackboard — новых полей
            // на юните заводить не нужно, вся нужная информация уже там.
            var blackboard = (enemy.Brain as UtilityUnitBrain)?.Blackboard;
            if (blackboard == null) return false;

            // Suspicious / Alerted / Combat — покрывает "in combat",
            // "currently attacking" и "recently damaged" (OnDamaged
            // выставляет AlertPhase.Alerted).
            if (blackboard.AlertPhase != AlertPhase.Calm) return true;

            // Зарезервирован PackCoordinator'ом — держит позицию в стае,
            // не должен пропасть из симуляции.
            if (blackboard.HasPackSlot) return true;

            // Услышал шум, но ещё не обработал его в думалке.
            if (blackboard.HeardNoise) return true;

            return false;
        }
    }
}