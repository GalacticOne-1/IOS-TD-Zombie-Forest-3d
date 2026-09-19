using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>State-семантика — см. TutorialStateRecheckObjectiveBase докстринг:
    /// ItemTransferredEvent здесь ТОЛЬКО триггер "перепроверь", источник истины всегда
    /// _amountQuery.GetAmount(). Не фильтруем событие по itemId/направлению перед
    /// перепроверкой — тот же приём, что SquadSizeObjective игнорирует payload
    /// StrategicSquadChangedEvent целиком: лишний recompute от несвязанного переноса
    /// безвреден, а GetAmount() сам по себе полностью самодостаточен и корректен.</summary>
    public sealed class ItemTransferredObjective : TutorialStateRecheckObjectiveBase<ItemTransferredEvent>
    {
        private readonly ITutorialInventorySourceAmountQuery _amountQuery;
        private readonly ItemId _itemId;
        private readonly InventorySourceType _toSourceType;
        private readonly int _requiredAmount;

        public ItemTransferredObjective(
            ITutorialInventorySourceAmountQuery amountQuery,
            ItemId itemId,
            InventorySourceType toSourceType,
            int requiredAmount)
        {
            _amountQuery = amountQuery;
            _itemId = itemId;
            _toSourceType = toSourceType;
            _requiredAmount = requiredAmount;
        }

        public override bool EvaluateCurrentState()
            => _amountQuery.GetAmount(_toSourceType, _itemId) >= _requiredAmount;

        public override bool TryGetProgress(out int current, out int required)
        {
            current = _amountQuery.GetAmount(_toSourceType, _itemId);
            required = _requiredAmount;
            return true;
        }
    }
}