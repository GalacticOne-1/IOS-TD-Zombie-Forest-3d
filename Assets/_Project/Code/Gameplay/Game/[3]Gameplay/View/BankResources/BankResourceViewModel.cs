
using R3;

namespace Galactic1
{
    public class BankResourceViewModel
    {
        public readonly EBankResourceType ResourceType;
        public readonly ReactiveProperty<int> Amount;

        public BankResourceViewModel(BankResourceProxy resource)
        {
            ResourceType = resource.BankResourceType;
            Amount = resource.Amount;
        }
    }
}