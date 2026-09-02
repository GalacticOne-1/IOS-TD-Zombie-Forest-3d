namespace Galactic1.Code.UI.Inventory
{
    public struct AddItemResult
    {
        public int Added;     // сколько реально вошло
        public int Remaining;  // сколько не поместилось

        public bool IsFullyAdded => Remaining == 0;

        public AddItemResult(int added, int remaining)
        {
            Added = added;
            Remaining = remaining;
        }
    }

}