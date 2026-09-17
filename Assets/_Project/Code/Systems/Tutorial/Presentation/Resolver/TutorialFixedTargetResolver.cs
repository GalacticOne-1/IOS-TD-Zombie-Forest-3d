
using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialFixedTargetResolver : ITutorialTargetResolver
    {
        private readonly TutorialTargetRegistry _registry;
        private readonly Dictionary<Action, Action<ITutorialTarget>> _handlers = new();

        public TutorialFixedTargetResolver(TutorialTargetRegistry registry) => _registry = registry;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialFixedTargetRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _registry.TryGetTarget(((TutorialFixedTargetRequest)request).TargetId, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            void Handler(ITutorialTarget registered) => callback();
            _handlers[callback] = Handler;
            _registry.OnTargetRegistered += Handler;
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_handlers.TryGetValue(callback, out var handler)) return;
            _registry.OnTargetRegistered -= handler;
            _handlers.Remove(callback);
        }
    }
}