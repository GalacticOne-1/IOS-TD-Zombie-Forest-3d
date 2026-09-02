namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Унифицированная шкала качества/объёма ресурсов.
    /// Используется для всех типов ресурсов и лута.
    /// </summary>
    public enum ResourceVolume
    {
        None = 0,       // отсутствует
        Unknown = 1,    // Нет данных
        VeryLow = 2,    // очень мало
        Low = 3,        // мало
        Medium = 4,     // средний объём
        High = 5,       // много
        VeryHigh = 6    // очень много
    }
}