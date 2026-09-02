namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Доминирующий тип противников в локации.
    /// Может использоваться для подбора экипировки.
    /// </summary>
    public enum EnemyCompositionType
    {
        None = 0,
        Unknown = 1,
        Wildlife = 2,     // Дикая фауна
        Raiders = 3,      // Бандиты / рейдеры
        Cultists = 4,     // Культисты
        Undead = 5,       // Нежить
        Military = 6,     // Организованные военные силы
        Robots = 7        // Автономные машины
    }
}