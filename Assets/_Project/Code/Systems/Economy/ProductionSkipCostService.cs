using Galactic1.Code.Systems.Economy.Configs;

namespace Galactic1.Code.Systems.Economy
{
    /// <summary>
    /// Рассчитывает стоимость ускорения производства.
    /// Не списывает валюту.
    /// Чистая формула.
    /// </summary>
    public sealed class ProductionSkipCostService
    {
        private readonly int CostPerHour = 3;
        private const int MinimumCost = 1;

        
        public ProductionSkipCostService(EconomyConfig economyConfig)
        {
            CostPerHour = economyConfig.ProductionCostPerHour;
        }

        public int Calculate(int remainingHours, int stationLevel)
        {
            if (remainingHours <= 0)
                return 0;

            int baseCost = remainingHours * CostPerHour;

            // Можно добавить модификаторы уровня
            int levelModifier = 1 + stationLevel;

            int final = baseCost * levelModifier;

            return final < MinimumCost ? MinimumCost : final;
        }
    }
}