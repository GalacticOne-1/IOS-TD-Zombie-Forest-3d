using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ItemSelectedGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialInventoryInteractionQuery _query;
        private readonly ItemId _itemId;
        private readonly bool _expectedSelected;
        private EventBinding<InventorySelectionChangedEvent> _selectBinding;
        private Action _onMightHaveChanged;

        public ItemSelectedGuidanceCondition(
            ITutorialInventoryInteractionQuery query, ItemId itemId, bool expectedSelected)
        {
            _query = query;
            _itemId = itemId;
            _expectedSelected = expectedSelected;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _selectBinding = new EventBinding<InventorySelectionChangedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<InventorySelectionChangedEvent>.Register(_selectBinding);
        }

        public void Stop()
        {
            if (_selectBinding != null) EventBus<InventorySelectionChangedEvent>.Deregister(_selectBinding);
            _selectBinding = null;
            _onMightHaveChanged = null;
        }

        // IsSatisfied() без изменений — по-прежнему чистый query, без локального _selected.
        public bool IsSatisfied() => _query.IsItemSelected(_itemId) == _expectedSelected;
    }
}