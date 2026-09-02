namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Тип и качество ожидаемого лута.
    /// Не гарантирует получение награды.
    /// </summary>
    public enum LootProfileType
    {
        None = 0,
        Unknown = 1,     // Нет данных    
        Poor = 2,        // Минимальные находки
        Common = 3,      // Обычные ресурсы
        Valuable = 4,    // Ценные предметы
        Rare = 5,        // Редкий лут
        Unique = 6       // Уникальные награды
    }
}