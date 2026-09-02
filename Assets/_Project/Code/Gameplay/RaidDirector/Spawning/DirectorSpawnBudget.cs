namespace Galactic1.Code.Gameplay.RaidDirector
{
    /// <summary>
    /// Отслеживает количество живых врагов заспавненных Director.
    ///
    /// v2: трекинг больше не требует HashSet или _pendingDirectorSpawns.
    /// Director определяет своих врагов через runtime.SpawnSource == SpawnSource.Director.
    /// Budget уменьшается в HandleEnemyDeath только если SpawnSource == Director.
    /// </summary>
    public sealed class DirectorSpawnBudget
    {
        private readonly DirectorConfig _config;

        public int CurrentAlive { get; private set; }

        public bool HasBudget => CurrentAlive < _config.MaxAliveFromDirector;

        public int Remaining => _config.MaxAliveFromDirector - CurrentAlive;

        public DirectorSpawnBudget(DirectorConfig config)
        {
            _config = config;
        }

        public void OnSpawned(int count)
        {
            CurrentAlive += count;
        }

        public void OnDirectorEnemyKilled()
        {
            if (CurrentAlive > 0)
                CurrentAlive--;
        }
    }
}