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
    /// ДОБАВЛЕНО:
    ///   1) Гистерезис радиусов — вход в более "живой" уровень по обычному
    ///      радиусу, выход обратно только после радиус + HysteresisMargin.
    ///      Отряд на границе больше не вызывает дрожание Full/Low/Sleeping.
    ///   2) Бюджет переходов на кадр — Evaluate() только строит очередь
    ///      нужных переходов, а применяются они в Tick() не более
    ///      MaxTransitionsPerFrame за кадр (дорогие Animator/AIPath enable
    ///      размазываются по кадрам). Приоритет: пробуждение ближайших первым,
    ///      засыпание — последним.
    /// </summary>
    public sealed class AILODSystem
    {
        // ── Очередь переходов ────────────────────────────────────────────
        private struct PendingTransition
        {
            public EnemyInstance Enemy;
            public int Category; // меньше = важнее (см. GetCategory)
            public float SqrDist; // внутри категории — ближайшие первыми
        }

        // Статический делегат — Sort не аллоцирует каждый раз.
        private static readonly Comparison<PendingTransition> PendingComparison = (a, b) =>
        {
            int c = a.Category.CompareTo(b.Category);
            return c != 0 ? c : a.SqrDist.CompareTo(b.SqrDist);
        };

        private readonly List<PendingTransition> _pending = new(64);
        private int _pendingCursor;

        private readonly AILODConfig _config;
        private readonly EnemyRepository _enemyRepository;
        private readonly RaidEnemyRegistry _enemyRegistry;
        private Func<Vector3> _squadCenterProvider;

        private readonly Dictionary<string, float> _spawnTime = new();
        private readonly Dictionary<string, SpawnSource> _spawnSource = new();

        // Радиусы входа (враг становится "живее")
        private readonly float _fullEnterSqr;

        private readonly float _lowEnterSqr;

        // Радиусы выхода (враг становится "тише") — больше на HysteresisMargin
        private readonly float _fullExitSqr;
        private readonly float _lowExitSqr;

        private readonly int _maxTransitionsPerFrame;

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

            float margin = Mathf.Max(0f, config.HysteresisMargin);

            _fullEnterSqr = config.FullSimulationRadius * config.FullSimulationRadius;
            _lowEnterSqr = config.LowSimulationRadius * config.LowSimulationRadius;

            float fullExit = config.FullSimulationRadius + margin;
            float lowExit = config.LowSimulationRadius + margin;
            _fullExitSqr = fullExit * fullExit;
            _lowExitSqr = lowExit * lowExit;

            _maxTransitionsPerFrame = Mathf.Max(1, config.MaxTransitionsPerFrame);

            _enemyRegistry.OnRegistered += HandleRegistered;
            _enemyRegistry.OnUnregistered += HandleUnregistered;
        }

        public void Initialize(Func<Vector3> squadCenterProvider)
        {
            _squadCenterProvider = squadCenterProvider;
            initialized = true;
        }

        /// <summary>
        /// Начальный проход когда все юниты в сцене готовы.
        /// Применяется БЕЗ бюджета: это загрузка, не геймплей.
        /// </summary>
        public void Entry()
        {
            squadCenter = ServiceLocator.Current.Get<CameraController>().FocusPosition;
            BuildQueue();
            ApplyAllImmediately();
        }

        public void Dispose()
        {
            _enemyRegistry.OnRegistered -= HandleRegistered;
            _enemyRegistry.OnUnregistered -= HandleUnregistered;

            _spawnTime.Clear();
            _spawnSource.Clear();
            _pending.Clear();
            _pendingCursor = 0;
        }

        // ── Registration ────────────────────────────────────────────────

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

            // 1) Раз в EvaluationInterval — пересобираем очередь.
            _evalTimer += dt;
            if (_evalTimer >= _config.EvaluationInterval)
            {
                _evalTimer = 0f;

                squadCenter = _squadCenterProvider != null
                    ? _squadCenterProvider()
                    : Vector3.zero;

                BuildQueue();
            }

            // 2) Каждый кадр — применяем не больше бюджета.
            DrainQueue(_maxTransitionsPerFrame);
        }

        // ── Queue building ───────────────────────────────────────────────

        /// <summary>
        /// Считает желаемый уровень для каждого врага и кладёт в очередь
        /// только тех, кому нужна смена. Ничего не применяет.
        /// Очередь пересобирается целиком — устаревшие записи не копятся.
        /// </summary>
        private void BuildQueue()
        {
            _pending.Clear();
            _pendingCursor = 0;

            float now = Time.time;

            foreach (var enemy in _enemyRepository.ActiveEnemies)
            {
                var runtime = enemy.EnemyAdapter?.Runtime;
                if (runtime == null) continue;

                var current = enemy.CurrentSimulationLevel;
                var desired = Decide(enemy, (EnemyRuntime)runtime, squadCenter, now, current);
                if (desired == current) continue;

                _pending.Add(new PendingTransition
                {
                    Enemy = enemy,
                    Category = GetCategory(current, desired),
                    SqrDist = (enemy.transform.position - squadCenter).sqrMagnitude
                });
            }

            if (_pending.Count > 1)
                _pending.Sort(PendingComparison);
        }

        /// <summary>
        /// 0 — пробуждение в Full (враг рядом, нужен срочно)
        /// 1 — Sleeping → Low
        /// 2 — Full → Low (понижение, не срочно)
        /// 3 — засыпание (самое неспешное)
        /// </summary>
        private static int GetCategory(SimulationLevel from, SimulationLevel to)
        {
            if (to == SimulationLevel.Full) return 0;
            if (to == SimulationLevel.Low && from == SimulationLevel.Sleeping) return 1;
            if (to == SimulationLevel.Low) return 2;
            return 3; // to == Sleeping
        }

        // ── Queue draining ───────────────────────────────────────────────

        private void DrainQueue(int budget)
        {
            if (_pendingCursor >= _pending.Count) return;

            float now = Time.time;
            int applied = 0;

            while (_pendingCursor < _pending.Count && applied < budget)
            {
                var item = _pending[_pendingCursor++];
                if (TryApply(item.Enemy, now))
                    applied++;
            }

            if (_pendingCursor >= _pending.Count)
            {
                _pending.Clear();
                _pendingCursor = 0;
            }
        }

        private void ApplyAllImmediately()
        {
            float now = Time.time;
            for (int i = 0; i < _pending.Count; i++)
                TryApply(_pending[i].Enemy, now);

            _pending.Clear();
            _pendingCursor = 0;
        }

        /// <summary>
        /// Уровень пересчитывается в момент применения: между постановкой
        /// в очередь и применением мог пройти не один кадр (враг умер,
        /// вступил в бой, отряд развернулся). Если смена уже не нужна —
        /// пропускаем, бюджет не тратится. Возвращает true, если реально
        /// применён переход.
        /// </summary>
        private bool TryApply(EnemyInstance enemy, float now)
        {
            if (enemy == null) return false; // Unity-null: объект уничтожен

            var runtime = enemy.EnemyAdapter?.Runtime;
            if (runtime == null) return false;

            var current = enemy.CurrentSimulationLevel;
            var desired = Decide(enemy, (EnemyRuntime)runtime, squadCenter, now, current);
            if (desired == current) return false;

            enemy.SetSimulationLevel(desired); // единственная точка исполнения
            return true;
        }

        // ── Decision ─────────────────────────────────────────────────────

        private SimulationLevel Decide(
            EnemyInstance enemy,
            EnemyRuntime runtime,
            Vector3 squadCenter,
            float now,
            SimulationLevel current)
        {
            // 1) Director grace period.
            if (_spawnSource.TryGetValue(runtime.Id, out var source) &&
                source == SpawnSource.Director)
            {
                float spawnedAt = _spawnTime.TryGetValue(runtime.Id, out var t) ? t : now;
                if (now - spawnedAt < _config.DirectorSpawnGracePeriod)
                    return SimulationLevel.Full;
            }

            // 2) Игровое состояние важнее дистанции.
            if (IsGameplayCritical(enemy))
                return SimulationLevel.Full;

            // 3) Дистанция с гистерезисом.
            //    Уже Full      -> остаётся Full до fullExit (радиус + margin).
            //    Не Full       -> становится Full только внутри fullEnter.
            //    Уже не спит   -> остаётся бодрым до lowExit.
            //    Спит          -> просыпается только внутри lowEnter.
            float sqrDist = (enemy.transform.position - squadCenter).sqrMagnitude;

            bool wasFull = current == SimulationLevel.Full;
            bool wasAwake = current != SimulationLevel.Sleeping;

            if (sqrDist <= (wasFull ? _fullExitSqr : _fullEnterSqr))
                return SimulationLevel.Full;

            if (sqrDist <= (wasAwake ? _lowExitSqr : _lowEnterSqr))
                return SimulationLevel.Low;

            return SimulationLevel.Sleeping;
        }

        private static bool IsGameplayCritical(EnemyInstance enemy)
        {
            var blackboard = (enemy.Brain as UtilityUnitBrain)?.Blackboard;
            if (blackboard == null) return false;

            if (blackboard.AlertPhase != AlertPhase.Calm) return true;
            if (blackboard.HasPackSlot) return true;
            if (blackboard.HeardNoise) return true;

            return false;
        }
    }
}