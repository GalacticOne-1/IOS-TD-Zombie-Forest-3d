namespace Galactic1.Code.Gameplay.Enemies.Spawning
{
    /// <summary>
    /// Источник спавна врага.
    /// Хранится в EnemySpawnRequest → пробрасывается в EnemyRuntime.
    ///
    /// Позволяет Director и другим системам различать своих врагов
    /// без HashSet'ов и счётчиков ожидания.
    /// </summary>
    public enum SpawnSource
    {
        Static, // ambient, расставленные на сцене
        Wave, // WaveSystem
        Director, // Raid Director
    }
}