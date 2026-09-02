using Galactic1;

namespace Galactic1
{
    public class CmdBankResourcesAdd : ICommand
    {
        public readonly EBankResourceType ResourceType;
        public readonly int Amount;

        public CmdBankResourcesAdd(EBankResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }
    }
}