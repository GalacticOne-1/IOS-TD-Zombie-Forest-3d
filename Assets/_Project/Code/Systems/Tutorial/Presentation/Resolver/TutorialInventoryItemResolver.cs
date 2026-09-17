// TutorialInventoryItemResolver.cs
using System;
using System.Collections.Generic;
using Galactic1.Mobile.EventBus;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialInventoryItemResolver : ITutorialTargetResolver
    {
        private readonly ITutorialItemSlotTargetProvider _provider;
        private readonly Dictionary<Action, EventBinding<InventoryContentsChangedEvent>> _bindings = new();

        public TutorialInventoryItemResolver(ITutorialItemSlotTargetProvider provider) => _provider = provider;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialInventoryItemRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _provider.TryGetSlotTarget(((TutorialInventoryItemRequest)request).ItemId, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var binding = new EventBinding<InventoryContentsChangedEvent>(_ => callback());
            _bindings[callback] = binding;
            EventBus<InventoryContentsChangedEvent>.Register(binding);
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_bindings.TryGetValue(callback, out var binding)) return;
            EventBus<InventoryContentsChangedEvent>.Deregister(binding);
            _bindings.Remove(callback);
        }
    }
}