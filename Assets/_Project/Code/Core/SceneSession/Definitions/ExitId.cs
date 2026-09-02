namespace Galactic1.Gameplay.Locations.Definitions
{
    /// <summary>
    /// Идентификатор зоны выхода на сцене.
    /// Используется только для логирования/дебага —
    /// на логику перехода не влияет (выход всегда в WorldMap).
    /// </summary>
    public enum ExitId
    {
        ExitNorth,
        ExitSouth,
        ExitWest,
        ExitEast,
        ExitGarage,
        Evacuation,
        Custom
    }
}