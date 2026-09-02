using Galactic1;

namespace Galactic1
{
    public class CmdCreateWorldState : ICommand
    {
        public readonly int WorldStateId;

        public CmdCreateWorldState(int worldStateId)
        {
            WorldStateId = worldStateId;
        }
    }
}