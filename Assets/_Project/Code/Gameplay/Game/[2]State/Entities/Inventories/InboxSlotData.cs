using System;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Inbox
{
    public class InboxSlotData : InventorySlotData
    {
        public string SlotId { get; set; }
        
        /// <summary>
        /// Мировой час когда слот истечёт
        /// </summary>
        public int ExpireWorldHour;
        
        public InboxSlotData(
            ItemConfig item,
            string itemKey, 
            int amount, 
            int durability,
            int ammoInMagazine,
            int expireWorldHour) 
            : base(item, itemKey, amount, durability, ammoInMagazine)
        {
            SlotId = Guid.NewGuid().ToString();
            ExpireWorldHour = expireWorldHour;
        }
    }
}