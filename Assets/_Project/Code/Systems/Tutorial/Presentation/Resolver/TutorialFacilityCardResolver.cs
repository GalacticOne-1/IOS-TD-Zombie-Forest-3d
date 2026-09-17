
using System;
using System.Collections.Generic;
using Galactic1.Code.UI.Construction;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialFacilityCardResolver : ITutorialTargetResolver
    {
        private readonly ITutorialFacilitySlotTargetProvider _provider;
        private readonly Dictionary<Action, Action> _handlers = new();

        public TutorialFacilityCardResolver(ITutorialFacilitySlotTargetProvider provider) => _provider = provider;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialFacilityCardRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _provider.TryGetFacilityTarget(((TutorialFacilityCardRequest)request).FacilityItemId, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var listView = controller?.View?.ListView;
            if (listView == null) return; // панель не открыта — валидный fallback "нечего пересчитывать"

            void Handler() => callback();
            _handlers[callback] = Handler;
            listView.OnRebuilt += Handler;
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_handlers.TryGetValue(callback, out var handler)) return;
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var listView = controller?.View?.ListView;
            if (listView != null) listView.OnRebuilt -= handler;
            _handlers.Remove(callback);
        }
    }
}