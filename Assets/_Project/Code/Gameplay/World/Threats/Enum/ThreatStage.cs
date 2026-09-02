namespace Galactic1.Code.Systems.World.Threats
{
    /// <summary>
    /// Стадия угрозы.
    /// Игрок видит только сигналы стадий, не скрытые счётчики.
    /// </summary>
    public enum ThreatStage
    {
        Dormant,      // угроза существует, игрок о ней не знает
        Brewing,      // активность зафиксирована, ранние сигналы
        Imminent,     // угроза может сработать при смене дня
        Active,       // угроза наступила, игрок должен реагировать
        Resolved,     // угроза устранена
        Missed
    }
}