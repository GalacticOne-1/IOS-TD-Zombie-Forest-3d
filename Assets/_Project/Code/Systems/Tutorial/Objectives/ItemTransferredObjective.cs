using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Event-семантика, тот же паттерн, что ItemCollectedObjective — счётчик
    /// накапливается от событий, пришедших ЗА ВРЕМЯ активного шага, ретроактивности нет
    /// (перемещение, случившееся до активации шага, не засчитывается).</summary>
    public sealed class ItemTransferredObjective : TutorialEventObjectiveBase<ItemTransferredEvent>
    {
        private readonly ItemId _itemId; // null = любой предмет
        private readonly InventorySourceType? _fromSourceType; // null = любой источник
        private readonly InventorySourceType? _toSourceType;   // null = любой источник
        private readonly int _requiredAmount;
        private int _transferred;

        public ItemTransferredObjective(
            ItemId itemId, 
            InventorySourceType? fromSourceType,
            InventorySourceType? toSourceType, 
            int requiredAmount)
        {
            _itemId = itemId;
            _fromSourceType = fromSourceType;
            _toSourceType = toSourceType;
            _requiredAmount = requiredAmount;
        }

        protected override bool EvaluateEvent(ItemTransferredEvent e)
        {
            if (_itemId != null && e.Slot.Item.Id != _itemId) return false;
            if (_fromSourceType.HasValue && e.FromSourceType != _fromSourceType.Value) return false;
            if (_toSourceType.HasValue && e.ToSourceType != _toSourceType.Value) return false;

            _transferred += e.Slot.Amount;
            return _transferred >= _requiredAmount;
        }

        public override bool TryGetProgress(out int current, out int required)
        {
            current = _transferred;
            required = _requiredAmount;
            return true;
        }
    }
}