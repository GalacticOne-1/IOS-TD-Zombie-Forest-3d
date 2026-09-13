using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ItemDraggedGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialInventoryInteractionQuery _query;
        private readonly ItemId _itemId;
        private readonly bool _expectedDragged;
        private EventBinding<InventoryDragStartedEvent> _dragBinding;
        private EventBinding<InventoryDragEndedEvent> _dragEndedBinding;
        private Action _onMightHaveChanged;

        public ItemDraggedGuidanceCondition(
            ITutorialInventoryInteractionQuery query, ItemId itemId, bool expectedDragged)
        {
            _query = query;
            _itemId = itemId;
            _expectedDragged = expectedDragged;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _dragBinding = new EventBinding<InventoryDragStartedEvent>(_ => _onMightHaveChanged?.Invoke());
            _dragEndedBinding = new EventBinding<InventoryDragEndedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<InventoryDragStartedEvent>.Register(_dragBinding);
            EventBus<InventoryDragEndedEvent>.Register(_dragEndedBinding);
        }

        public void Stop()
        {
            if (_dragBinding != null) EventBus<InventoryDragStartedEvent>.Deregister(_dragBinding);
            if (_dragEndedBinding != null) EventBus<InventoryDragEndedEvent>.Deregister(_dragEndedBinding);
            _dragBinding = null;
            _dragEndedBinding = null;
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied() => _query.IsItemBeingDragged(_itemId) == _expectedDragged;
    }
}