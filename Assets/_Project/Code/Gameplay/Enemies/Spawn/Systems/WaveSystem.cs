using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>
    /// Оркестратор волн Camp Defense.
    ///
    /// НЕ хранит прогресс — это WaveProgressRuntime.
    /// НЕ знает про Mission/RaidStatus/GameLoopState — сообщает наружу
    /// только через EventBus (WaveCompletedEvent / AllWavesCompletedEvent).
    /// НЕ ищет врагов через RaidEnemyRegistry.All/AliveCount — отслеживает
    /// только своих (SpawnSource.Wave) через события OnRegistered/EnemyKilledEvent.
    /// НЕ зависит от CampDefenseScenario — тот вызывает только StartFirstWave()/Tick().
    /// </summary>
    public sealed class WaveSystem
    {
        private readonly WaveConfig _config;
        private readonly EnemySpawnSystem _spawnSystem;
        private readonly RaidEnemyRegistry _enemies;
        private readonly WaveSpawnPointResolver _pointResolver;
        private readonly WaveProgressRuntime _progress;
        private readonly EnemySpawnSystem _enemySpawnSystem;

        private readonly EventBinding<EnemyKilledEvent> _killedBinding;

        private List<InstructionRuntime> _activeInstructions;
        private bool _started;

        public WaveSystem(
            WaveConfig config,
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

            // Спавн волны закончен, когда все инструкции отдали своих врагов в очередь.
            // Срабатывает ровно один раз благодаря IsSpawningFinished.
            if (_progress.PendingInstructions == 0 && !_progress.IsSpawningFinished)
            {
                CompleteCurrentWave();
            }

            // AllWavesCompletedEvent — тоже ровно один раз.
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

        // ── Спавн: только Enqueue. Очередь обрабатывает EnemySpawnSystem.Tick() сам, как обычно. ──

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
                instr.EntryCountdown = group.Enemies[instr.EntryCursor].Count;
            }
        }

        private void MarkInstructionFinished(InstructionRuntime instr)
        {
            instr.Finished = true;
            _progress.NotifyInstructionFinished();
        }

        // ── Отслеживание только своих (SpawnSource.Wave) врагов ──────────────

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

        // ── Переходы между волнами — только события, никаких прямых решений о миссии ──

        /// <summary>
        /// Означает "спавнер данной волны полностью закончил свою работу" —
        /// НЕ "пришла следующая волна". Индекс волны здесь не трогаем:
        /// его меняет только AdvanceToWave() → _progress.StartWave().
        /// </summary>
        private void CompleteCurrentWave()
        {
            _progress.CompleteSpawning(); // сначала выставляет IsFinished, если это была последняя волна
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
                _activeInstructions.Add(new InstructionRuntime(instruction));

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

        // ── Тайминг конкретной инструкции. Это НЕ прогресс волны —
        //    это внутренняя механика "когда именно спавнить следующего врага". ──

        private sealed class InstructionRuntime
        {
            public readonly WaveSpawnInstruction Definition;
            public readonly int TotalCount;

            public float ElapsedSinceUnlocked;
            public float IntervalTimer;
            public int EntryCursor;
            public int EntryCountdown;
            public int SpawnedCount;
            public bool Finished;

            public InstructionRuntime(WaveSpawnInstruction definition)
            {
                Definition = definition;

                int total = 0;
                foreach (var e in definition.Group.Enemies)
                    total += e.Count;
                TotalCount = total;

                Finished = TotalCount == 0;
                EntryCountdown = definition.Group.Enemies.Count > 0
                    ? definition.Group.Enemies[0].Count
                    : 0;
            }
        }
    }
}