
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// DTO одного предмета во входящих.
    /// </summary>
    public class InboxItemDTO
    {
        public string SlotId;

        public ItemConfig Item;

        public int Count;

        
        public int Durability;
        public float Durability01;
        
        public int RemainingHours;
    }
}