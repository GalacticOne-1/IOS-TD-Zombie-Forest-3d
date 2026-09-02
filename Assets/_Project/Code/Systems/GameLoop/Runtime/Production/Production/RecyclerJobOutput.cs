using System;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Один ресурс, который возвращает Recycler.
    /// </summary>
    [Serializable]
    public struct RecyclerJobOutput
    {
        public string ItemId;
        public int Amount;

        public RecyclerJobOutput(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}