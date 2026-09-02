namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Режим переработки ресурсов.
    /// Влияет на формирование output (баланс рассчитывается в другом сервисе).
    /// </summary>
    public enum ProcessingMode
    {
        Standard,
        Bulk,
        Precision
    }
}