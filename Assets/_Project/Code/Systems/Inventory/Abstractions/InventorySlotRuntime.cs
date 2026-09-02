using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Inventory.Abstractions
{
    /// <summary>
    /// Runtime-версия слота инвентаря, используемая только во время рейда.
    /// Не связана с Proxy и не сериализуется.
    /// </summary>
    public sealed class InventorySlotRuntime
    {
        public ItemConfig Item;
        public int Amount;          // кол-во предмета
        
        public int Durability;      // прочность предмета
        
        /// Загруженные патроны в оружии (например 12/30)
        public int AmmoInMagazine; 

        public InventorySlotRuntime(
            ItemConfig item,
            int amount,
            int durability,
            int ammoInMagazine)
        {
            Item = item;
            Amount = amount;
            Durability = durability;
            AmmoInMagazine = ammoInMagazine;
        }

        public bool IsEmpty => Item == null || Amount <= 0;

        public InventorySlotRuntime Clone() => new(Item, Amount, Durability, AmmoInMagazine);
    }
}