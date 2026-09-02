namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Операционные риски и требования для посещения локации.
    /// Используется в стратегическом планировании.
    /// </summary>
    public enum OperationalRiskLevel
    {
        Unknown = 0,
        Minimal = 1,
        Manageable = 2,
        Dangerous = 3,
        Critical = 4
    }
}