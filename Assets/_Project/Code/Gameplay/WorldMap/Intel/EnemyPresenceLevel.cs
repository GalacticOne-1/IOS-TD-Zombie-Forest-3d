namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Плотность вражеского присутствия в локации.
    /// Используется для оценки масштабов столкновений.
    /// </summary>
    public enum EnemyPresenceLevel
    {
        None = 0,
        Unknown = 1,
        Sparse = 2,    // Редкие враги
        Moderate = 3,  // Умеренное присутствие
        Heavy = 4,     // Плотное присутствие
        Overrun = 5    // Локация захвачена врагами
    }
}