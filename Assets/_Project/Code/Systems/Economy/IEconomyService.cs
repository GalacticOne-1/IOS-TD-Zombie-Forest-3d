using R3;

namespace Galactic1.Code.Systems.Economy
{
    public interface IEconomyService : IGameService
    {
        public Observable<int> ObservResource(EBankResourceType resourceType);
        
        int GetBalance(EBankResourceType type);
        bool HasEnough(EBankResourceType type, int amount);
        void Add(EBankResourceType type, int amount);
        bool TrySpend(EBankResourceType type, int amount);
        
        
        int CalculateProductionSkipCost(int remainingHours, int stationLevel);
        
        int CalculateDroneSendCost();
        int CalculateDroneExtraChargeCost();
    }
}