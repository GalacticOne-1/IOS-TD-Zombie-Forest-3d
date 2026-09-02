using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using R3;

namespace Galactic1
{
    public class InventorySlotProxy
    {
        public readonly InventorySlotData Origin;

        public readonly ReactiveProperty<ItemConfig> Item;
        public readonly ReactiveProperty<int> Amount;
        public readonly ReactiveProperty<int> Durability;
        public readonly ReactiveProperty<int> AmmoInMagazine;

        public bool IsEmpty => Item.Value == null || Amount.Value <= 0;

        public InventorySlotProxy(InventorySlotData data)
        {
            Origin = data;

            // здесь slot хранит сам ItemBase
            Item = new(Origin.Item);
            Amount = new(Origin.Amount);
            Durability = new(Origin.Durability);
            AmmoInMagazine = new(Origin.AmmoInMagazine);

            // Перепривязываем Item/Amount
            BindToSave(Origin);
        }
        
        public void BindToSave(InventorySlotData origin)
        {
            Item.Subscribe(_ => origin.ItemKey = _?.Id.Guid ?? "");
            Amount.Subscribe(_ => origin.Amount = _);
            Durability.Subscribe(_ => origin.Durability = _);
            AmmoInMagazine.Subscribe(_ => origin.AmmoInMagazine = _);
        }

        public bool CanStack(ItemConfig newItem)
        {
            return !IsEmpty &&
                   Item.Value == newItem &&
                   Amount.Value < newItem.Classification.maxStack;
        }

        public void Clear()
        {
            Item.Value = null;
            Amount.Value = 0;
            Durability.Value = 0;
            AmmoInMagazine.Value = 0;
        }
    }

}