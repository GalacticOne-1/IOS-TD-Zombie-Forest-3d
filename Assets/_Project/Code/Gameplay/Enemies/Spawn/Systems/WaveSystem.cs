using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    public sealed class WaveSystem
    {
        private readonly WaveConfig _config;
        private readonly EnemySpawnSystem _spawnSystem;
        private readonly RaidEnemyRegistry _enemies;
        private readonly WaveSpawnPointResolver _pointResolver;
        private readonly WaveProgressRuntime _progress;
        private readonly EnemySpawnSystem _enemySpawnSystem;

        private readonly float _waveScale;
        private readonly HashSet<EnemyId> _excludedEnemyIds;

        private readonly EventBinding<EnemyKilledEvent> _killedBinding;

        private List<InstructionRuntime> _activeInstructions;
        private bool _started;

        public WaveSystem(
            WaveConfig config,
            WaveDifficultyResolution difficulty,
            EnemySpawnSystem spawnSystem,
            RaidEnemyRegistry enemies,
            WaveSpawnPointResolver pointResolver,
            WaveProgressRuntime progress,
            EnemySpawnSystem enemySpawnSystem)
        {
            _config = config;
            _spawnSystem = spawnSystem;
            _enemies = enemies;
            _pointResolver = pointResolver;
            _progress = progress;
            _enemySpawnSystem = enemySpawnSystem;

            _waveScale = Mathf.Clamp01(difficulty.Scale);
            _excludedEnemyIds = new HashSet<EnemyId>(difficulty.ExcludedEnemyIds);

            _progress.Configure(_config.Waves.Count);

            _enemies.OnRegistered += HandleEnemyRegistered;

            _killedBinding = new EventBinding<EnemyKilledEvent>(HandleEnemyKilled);
            EventBus<EnemyKilledEvent>.Register(_killedBinding);
        }

        public void StartFirstWave()
        {
            if (_started) return;
            _started = true;
            AdvanceToWave(0);
        }

        /// <summary>Вызывается из RaidRuntime.Tick() каждый кадр.</summary>
        public void Tick(float dt)
        {
            if (!_started || !_progress.IsWaveRunning) return;

            _progress.Tick(dt);
            _enemySpawnSystem.Tick(dt);

            for (int i = 0; i < _activeInstructions.Count; i++)
            {
                var instr = _activeInstructions[i];
                if (instr.Finished) continue;

                if (instr.Definition.WaitPreviousInstruction &&
                    i > 0 && !_activeInstructions[i - 1].Finished)
                    continue;

                instr.ElapsedSinceUnlocked += dt;
                if (instr.ElapsedSinceUnlocked < instr.Definition.Delay)
                    continue;

                instr.IntervalTimer -= dt;
                if (instr.IntervalTimer > 0f) continue;

                SpawnNext(instr);
                instr.IntervalTimer = Mathf.Max(instr.Definition.Interval, 0f);

                if (instr.SpawnedCount >= instr.TotalCount)
                    MarkInstructionFinished(instr);
            }

            if (_progress.PendingInstructions == 0 && !_progress.IsSpawningFinished)
            {
                CompleteCurrentWave();
            }

            if (!_progress.IsDefenseCompleted &&
                _progress.CanFinishAllWaves())
            {
                _progress.MarkDefenseCompleted();

                EventBus<AllWavesCompletedEvent>.Raise(new AllWavesCompletedEvent());
            }

            if (_progress.CanStartNextWave())
            {
                AdvanceToWave(_progress.CurrentWaveIndex + 1);
            }
        }

        private void SpawnNext(InstructionRuntime instr)
        {
            var group = instr.Definition.Group;
            var entry = group.Enemies[instr.EntryCursor];

            var basePosition = _pointResolver.Resolve(instr.Definition.SpawnPointId);

            _spawnSystem.Enqueue(new EnemySpawnRequest(
                entry.Enemy.Id,
                basePosition,
                "",
                null,
                _progress.CurrentWaveIndex,
                SpawnSource.Wave));

            instr.SpawnedCount++;
            instr.EntryCountdown--;

            while (instr.EntryCountdown <= 0 && instr.EntryCursor < group.Enemies.Count - 1)
            {
                instr.EntryCursor++;
                instr.EntryCountdown = instr.EntryCounts[instr.EntryCursor];
            }
        }

        private void MarkInstructionFinished(InstructionRuntime instr)
        {
            instr.Finished = true;
            _progress.NotifyInstructionFinished();
        }

        private void HandleEnemyRegistered(EnemyRuntime runtime)
        {
            if (runtime.SpawnSource != SpawnSource.Wave) return;
            _progress.RegisterSpawn(runtime);
        }

        private void HandleEnemyKilled(EnemyKilledEvent e)
        {
            if (e.Runtime.SpawnSource != SpawnSource.Wave) return;
            _progress.RegisterDeath(e.Runtime);
        }

        private void CompleteCurrentWave()
        {
            _progress.CompleteSpawning();
            EventBus<WaveCompletedEvent>.Raise(new WaveCompletedEvent
            {
                AllWavesCompleted = _progress.IsFinished
            });
        }

        private void AdvanceToWave(int index)
        {
            var wave = _config.Waves[index];

            _activeInstructions = new List<InstructionRuntime>(wave.Instructions.Count);
            foreach (var instruction in wave.Instructions)
                _activeInstructions.Add(new InstructionRuntime(instruction, _waveScale, _excludedEnemyIds));

            int pending = 0;
            foreach (var instr in _activeInstructions)
                if (!instr.Finished)
                    pending++;

            _progress.StartWave(index, wave, pending);
        }

        public void Dispose()
        {
            _enemies.OnRegistered -= HandleEnemyRegistered;
            EventBus<EnemyKilledEvent>.Deregister(_killedBinding);
        }

        private sealed class InstructionRuntime
        {
            public readonly WaveSpawnInstruction Definition;
            public readonly int TotalCount;
            public readonly int[] EntryCounts;

            public float ElapsedSinceUnlocked;
            public float IntervalTimer;
            public int EntryCursor;
            public int EntryCountdown;
            public int SpawnedCount;
            public bool Finished;

            public InstructionRuntime(
                WaveSpawnInstruction definition,
                float waveScale,
                HashSet<EnemyId> excludedEnemyIds)
            {
                Definition = definition;

                var enemies = definition.Group.Enemies; // List<AmbientEnemyEntry> — не мутируем
                EntryCounts = new int[enemies.Count];

                int total = 0;
                for (int i = 0; i < enemies.Count; i++)
                {
                    var enemyEntry = enemies[i];

                    // Исключённые для текущего диапазона уровней типы не спавнятся вовсе.
                    bool isExcluded = enemyEntry.Enemy != null &&
                                      excludedEnemyIds.Contains(enemyEntry.Enemy.Id);

                    int scaledCount = isExcluded
                        ? 0
                        : Mathf.CeilToInt(enemyEntry.Count * waveScale);

                    EntryCounts[i] = scaledCount;
                    total += scaledCount;
                }

                TotalCount = total;
                Finished = TotalCount == 0;

                // Пропускаем ведущие исключённые/нулевые записи, чтобы курсор сразу
                // указывал на первую реально спавнящуюся запись группы.
                EntryCursor = 0;
                while (EntryCursor < EntryCounts.Length - 1 && EntryCounts[EntryCursor] == 0)
                    EntryCursor++;

                EntryCountdown = EntryCounts.Length > 0
                    ? EntryCounts[EntryCursor]
                    : 0;
            }
        }
    }
}