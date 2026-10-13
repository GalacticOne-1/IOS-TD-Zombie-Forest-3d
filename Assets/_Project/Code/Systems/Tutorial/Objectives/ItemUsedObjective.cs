using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// "Предмет itemId использован (потрачен) ЗА ВРЕМЯ ЭТОГО ШАГА". Не ретроактивен в
    /// обычном смысле — но опционально (skipIfNotEnoughItems) может ретроактивно
    /// ЗАВЕРШИТЬСЯ, если на старте шага у игрока УЖЕ нет нужного количества предмета
    /// (значит, он потратил его раньше, до этого шага, и требовать повторного
    /// использования было бы некорректно/невозможно).
    /// </summary>
    public sealed class ItemUsedObjective : TutorialEventObjectiveBase<ItemConsumabledEvent>
    {
        private readonly ITutorialInventorySourceAmountQuery _amountQuery;
        private readonly ItemId _itemId; // null = любой предмет
        private readonly int _requiredCount;
        private readonly bool _skipIfNotEnoughItems;
        private readonly InventorySourceType _sourceA;
        private readonly InventorySourceType _sourceB;
        private int _current;

        public ItemUsedObjective(
            ITutorialInventorySourceAmountQuery amountQuery,
            ItemId itemId,
            int requiredCount,
            bool skipIfNotEnoughItems,
            InventorySourceType sourceA,
            InventorySourceType sourceB)
        {
            _amountQuery = amountQuery;
            _itemId = itemId;
            _requiredCount = requiredCount < 1 ? 1 : requiredCount;
            _skipIfNotEnoughItems = skipIfNotEnoughItems;
            _sourceA = sourceA;
            _sourceB = sourceB;
        }

        /// <summary>skipIfNotEnoughItems=false — прежнее поведение (всегда false, базовый
        /// класс не завершает объектив на старте). skipIfNotEnoughItems=true и itemId задан
        /// (для "любого предмета" проверка количества бессмысленна — пропускаем) — завершаем
        /// объектив сразу, если у игрока сейчас меньше requiredAmountCheck предметов.</summary>
        public override bool EvaluateCurrentState()
        {
            if (!_skipIfNotEnoughItems || _itemId == null)
                return false;

            int total = _amountQuery.GetAmount(_sourceA, _itemId);
            if (_sourceB != _sourceA)
                total += _amountQuery.GetAmount(_sourceB, _itemId);

            return total < _requiredCount;
        }

        protected override bool EvaluateEvent(ItemConsumabledEvent e)
        {
            if (_itemId != null && e.ItemId != _itemId)
                return false;

            _current += e.Amount;
            return _current >= _requiredCount;
        }

        public override bool TryGetProgress(out int current, out int required)
        {
            current = _current;
            required = _requiredCount;
            return _requiredCount > 1;
        }
    }
}