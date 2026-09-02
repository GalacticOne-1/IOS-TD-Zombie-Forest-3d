using R3;

namespace Galactic1
{
    public class BankResourceProxy
    {
        public readonly BankResourceData Origin;
        public EBankResourceType BankResourceType => Origin.BankResourceType;

        public readonly ReactiveProperty<int> Amount;

        public BankResourceProxy(BankResourceData origin)
        {
            Origin = origin;

            // R3
            Amount = new(origin.Amount);
            
            // subscription
            Amount.Subscribe(_ => origin.Amount = _);
        }
    }
}