namespace Galactic1.Code.Systems.World.Threats
{
    /// <summary>
    /// Тип угрозы в мире.
    /// Абстракция — не привязана к конкретным врагам.
    /// </summary>
    public enum ThreatType
    {
        Horde,        // орда зомби
        Cultists,     // нападение культистов
        EnemyCamp,    // вражеский лагерь
        WildBeasts,   // хищники / монстры
        Custom        // любой новый тип угрозы
    }
}