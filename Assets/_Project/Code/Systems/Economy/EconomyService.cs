using System;
using R3;

namespace Galactic1.Code.Systems.Economy
{
    /// <summary>
    /// Глобальный фасад экономики.
    /// Централизует доступ к валютам.
    /// Управляет транзакциями.
    /// </summary>
    public sealed class EconomyService : IEconomyService
    {
        private readonly CurrencyRuntime _currency;
        private readonly ProductionSkipCostService _skipCostService;
        private readonly DroneCostService _droneCostService;

        public event Action OnEconomyChanged;

        public EconomyService(
            CurrencyRuntime currency,
            ProductionSkipCostService skipCostService,
            DroneCostService droneCostService)
        {
            _currency = currency;
            _skipCostService = skipCostService;
            _droneCostService = droneCostService;

            _currency.OnChanged += RaiseChanged;
        }

        
        public Observable<int> ObservResource(EBankResourceType resourceType)
            => _currency.ObservResource(resourceType);

        // =========================================================
        // BALANCE
        // =========================================================

        public int GetBalance(EBankResourceType type)
        {
            return _currency.GetBalance(type);
        }

        public bool HasEnough(EBankResourceType type, int amount)
        {
            return _currency.GetBalance(type) >= amount;
        }

        // =========================================================
        // ADD / SPEND
        // =========================================================

        public void Add(EBankResourceType type, int amount)
        {
            if (amount <= 0)
                return;

            _currency.Add(type, amount);
        }

        public bool TrySpend(EBankResourceType type, int amount)
        {
            if (amount <= 0)
                return false;

            return _currency.TrySpend(type, amount);
        }

        // =========================================================
        // COST CALCULATION (Pure)
        // =========================================================

        public int CalculateProductionSkipCost(int remainingHours, int stationLevel)
            => _skipCostService.Calculate(remainingHours, stationLevel);


        public int CalculateDroneSendCost() => _droneCostService.CalculateSendCost();
        public int CalculateDroneExtraChargeCost() => _droneCostService.CalculateExtraChargeCost();

        // =========================================================

        private void RaiseChanged()
        {
            OnEconomyChanged?.Invoke();
        }
    }
}