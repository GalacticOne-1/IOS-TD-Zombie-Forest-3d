namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>
    /// Правило, по которому волна считается завершённой.
    /// Хранится в WaveDefinition — оценивает WaveProgressRuntime.CanFinishWave().
    /// </summary>
    public enum WaveCompletionMode
    {
        /// <summary>Все инструкции отспавнены и все Wave-враги мертвы.</summary>
        AllEnemiesDead,

        /// <summary>Волна длится WaveDefinition.Duration секунд, независимо от того, живы ли враги.</summary>
        TimerOnly,

        /// <summary>Волна завершается сразу после того, как все инструкции отдали своих врагов в очередь (без ожидания смертей).</summary>
        SpawnFinished,

        /// <summary>Зарезервировано для будущих сценарных правил — WaveProgressRuntime.CanFinishWave() для этого режима всегда возвращает false, решение принимает внешний код.</summary>
        Custom
    }
}