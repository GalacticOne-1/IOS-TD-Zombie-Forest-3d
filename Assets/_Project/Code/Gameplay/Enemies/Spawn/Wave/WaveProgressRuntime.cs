using System.Collections.Generic;
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>
    /// Единственный источник истины о прогрессе волн Camp Defense.
    ///
    /// WaveSystem — оркестратор: решает КОГДА спавнить и переключать волну.
    /// Само состояние (кто жив из МОИХ врагов, сколько инструкций осталось)
    /// хранится здесь и нигде больше.
    ///
    /// Никогда не обращается к RaidEnemyRegistry напрямую — данные приходят
    /// снаружи через RegisterSpawn/RegisterDeath, вызываемые WaveSystem
    /// в ответ на события.
    /// </summary>
    public sealed class WaveProgressRuntime
    {
        public int TotalWaves { get; private set; }
        public int CurrentWaveIndex { get; private set; } = -1;
        public WaveDefinition ActiveWave { get; private set; }

        public int SpawnedEnemies { get; private set; }

        private readonly HashSet<string> _aliveIds = new();
        public int AliveEnemies => _aliveIds.Count;

        public int PendingInstructions { get; private set; }
        public float ElapsedInWave { get; private set; }

        public bool IsWaveRunning { get; private set; }
        public bool IsFinished { get; private set; }

        public bool IsSpawningFinished { get; private set; }
        public bool IsDefenseCompleted { get; private set; }

        public bool WaitingForNextWave { get; private set; }

        /// <summary>Защита от повторного Raise(AllWavesCompletedEvent).</summary>
        public bool CompletionRaised { get; private set; }

        public void Configure(int totalWaves) => TotalWaves = totalWaves;

        public void StartWave(int index, WaveDefinition wave, int pendingInstructions)
        {
            CurrentWaveIndex = index;
            ActiveWave = wave;
            SpawnedEnemies = 0;
            ElapsedInWave = 0f;
            _aliveIds.Clear();
            PendingInstructions = pendingInstructions;
            IsWaveRunning = true;
            IsSpawningFinished = false;
            WaitingForNextWave = false;
        }

        public void Tick(float dt)
        {
            if (IsWaveRunning)
                ElapsedInWave += dt;
        }

        public void RegisterSpawn(EnemyRuntime runtime)
        {
            SpawnedEnemies++;
            _aliveIds.Add(runtime.Id);
        }

        public void RegisterDeath(EnemyRuntime runtime)
        {
            _aliveIds.Remove(runtime.Id);
        }

        /// <summary>Вызывается WaveSystem, когда инструкция отдала всех своих врагов в очередь.
        /// Только уменьшает счётчик — окончание спавна волны решает CompleteSpawning().</summary>
        public void NotifyInstructionFinished()
        {
            if (PendingInstructions > 0)
                PendingInstructions--;
        }

        /// <summary>Вызывается WaveSystem ровно один раз на волну, когда PendingInstructions == 0.
        /// Означает "спавнер этой волны закончил работу" — не "пришла следующая волна".</summary>
        public void CompleteSpawning()
        {
            IsSpawningFinished = true;
            WaitingForNextWave = true;
            ElapsedInWave = 0f;

            if (CurrentWaveIndex + 1 >= TotalWaves)
                IsFinished = true;   // означает только:
            // "последняя волна заспавнилась"
        }

        public bool CanStartNextWave()
        {
            if (!WaitingForNextWave || IsFinished)
                return false;

            return ElapsedInWave >= ActiveWave.DelayBeforeNextWave;
        }

        public bool CanFinishAllWaves()
        {
            if (!IsFinished)
                return false;

            if (AliveEnemies > 0)
                return false;

            return true;
        }

        public void MarkDefenseCompleted() => IsDefenseCompleted = true;

        public void MarkCompletionRaised() => CompletionRaised = true;
    }
}