using Galactic1;

namespace Galactic1
{
    public class CmdBankResourcesSpend : ICommand
    {
        public readonly EBankResourceType ResourceType;
        public readonly int Amount;

        public CmdBankResourcesSpend(EBankResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }
    }
}