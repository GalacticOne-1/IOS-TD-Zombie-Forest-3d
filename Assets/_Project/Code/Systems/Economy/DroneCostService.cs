
using Galactic1.Code.Systems.Economy.Configs;

namespace Galactic1.Code.Systems.Economy
{
    /// <summary>
    /// Рассчитывает стоимость операций дрона.
    /// Не списывает валюту.
    /// Чистая формула.
    /// </summary>
    public sealed class DroneCostService
    {
        private readonly int _sendCost;
        private readonly int _extraChargeCost;

        public DroneCostService(EconomyConfig config)
        {
            _sendCost = config.CargoDroneCostPremium;
            // _extraChargeCost = config.DroneExtraChargeCost;
        }

        /// <summary>
        /// Стоимость одной отправки дрона за премиум.
        /// </summary>
        public int CalculateSendCost() => _sendCost;

        /// <summary>
        /// Стоимость одного дополнительного вылета сверх лимита.
        /// </summary>
        public int CalculateExtraChargeCost() => _extraChargeCost;
    }
}