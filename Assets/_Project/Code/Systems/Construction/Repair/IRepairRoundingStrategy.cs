namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Централизованная стратегия округления стоимости ремонта.
    /// Позволяет переопределить правило (Ceil/Floor/Round) в одном месте.
    /// </summary>
    public interface IRepairRoundingStrategy
    {
        int Round(float amount);
    }
}