
using System;
using System.Collections.Generic;
using Galactic1.Code.UI.Construction;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialConstructionTabResolver : ITutorialTargetResolver
    {
        private readonly ITutorialConstructionTabTargetProvider _provider;
        private readonly Dictionary<Action, Action> _handlers = new();

        public TutorialConstructionTabResolver(ITutorialConstructionTabTargetProvider provider) => _provider = provider;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialConstructionTabRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _provider.TryGetTabTarget(((TutorialConstructionTabRequest)request).Category, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var view = controller?.View;
            if (view == null) return;

            void Handler() => callback();
            _handlers[callback] = Handler;
            view.OnTabsRebuilt += Handler;
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_handlers.TryGetValue(callback, out var handler)) return;
            var controller = ServiceLocator.Current.Get<ConstructionPanelController>();
            var view = controller?.View;
            if (view != null) view.OnTabsRebuilt -= handler;
            _handlers.Remove(callback);
        }
    }
}