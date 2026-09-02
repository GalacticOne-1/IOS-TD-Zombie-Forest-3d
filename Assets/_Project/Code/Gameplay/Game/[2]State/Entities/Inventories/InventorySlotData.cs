
using Galactic1.Game.Meta.Items;

namespace Galactic1
{
    [System.Serializable]
    public class InventorySlotData
    {
        public string ItemKey { get; set; }
        public int Amount { get; set; }
        public int Durability { get; set; }
        
        public int AmmoInMagazine { get; set; }

        [System.NonSerialized] 
        public ItemConfig Item;

        public InventorySlotData(
            ItemConfig item, 
            string itemKey, 
            int amount, 
            int durability, 
            int ammoInMagazine)
        {
            Item = item;
            ItemKey = itemKey;
            Amount = amount;
            Durability = durability;
            AmmoInMagazine = ammoInMagazine;
        }
    }
}