using Galactic1;

namespace Galactic1
{
    public class CmdCrates : ICommand
    {
        public readonly bool Unlock;
        public readonly int SlotAmount;

        public CmdCrates(bool unlock, int slotAmount)
        {
            Unlock = unlock;
            SlotAmount = slotAmount;
        }
    }
}