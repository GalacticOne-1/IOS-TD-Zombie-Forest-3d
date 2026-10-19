using Galactic1.Code.Systems.Economy.Configs;

namespace Galactic1.Code.Systems.Economy
{
    /// <summary>
    /// Рассчитывает стоимость ускорения производства.
    /// Не списывает валюту.
    /// Чистая формула.
    ///
    /// Правило: 24 часа = 10 монет, минимум 1 монета, округление всегда вниз.
    /// Примеры: 12 ч = 5, 8 ч = 3 (3.33), 3 ч = 1 (1.25 -> 1), 1 ч = 1 (0.41 -> минимум 1),
    ///          14 ч = 5 (5.83 -> 5), 29 ч = 12 (12.08 -> 12).
    /// </summary>
    public sealed class ProductionSkipCostService
    {
        private const int HoursPerPeriod = 24;
        private const int CoinsPerPeriod = 10;
        private const int MinimumCost = 1;

        // Параметр оставлен, чтобы не ломать регистрацию в DI.
        // Цена теперь задаётся константами выше, а не EconomyConfig.ProductionCostPerHour.
        public ProductionSkipCostService(EconomyConfig economyConfig)
        {
        }

        public int Calculate(int remainingHours, int stationLevel)
        {
            if (remainingHours <= 0)
                return 0;

            // целочисленное деление = округление вниз
            int cost = remainingHours * CoinsPerPeriod / HoursPerPeriod;

            return cost < MinimumCost ? MinimumCost : cost;
        }
    }
}