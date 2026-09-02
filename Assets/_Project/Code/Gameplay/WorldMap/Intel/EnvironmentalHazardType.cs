namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Опасные условия окружающей среды.
    /// Может влиять на бой и время рейда.
    /// </summary>
    public enum EnvironmentalHazardType
    {
        None = 0,
        Unknown = 1,            // Нет данных
        Radiation = 2,
        ToxicAtmosphere = 3,
        ExtremeCold = 4,
        ExtremeHeat = 5,
        LowVisibility = 6
    }
}