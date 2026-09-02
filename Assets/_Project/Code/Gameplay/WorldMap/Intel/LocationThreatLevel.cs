namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Общий уровень угрозы локации.
    /// Отражает риск для игрока, а не точную боевую сложность.
    /// </summary>
    public enum LocationThreatLevel
    {
        None = 0,
        Unknown = 1,   // Нет данных
        Low = 2,       // Низкая угроза
        Medium = 3,    // Средняя угроза
        High = 4,      // Высокая угроза
        Extreme = 5    // Критическая угроза
    }
}