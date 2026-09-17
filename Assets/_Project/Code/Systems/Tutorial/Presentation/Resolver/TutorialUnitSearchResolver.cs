
using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialUnitSearchResolver : ITutorialTargetResolver
    {
        private readonly ITutorialUnitSlotTargetProvider _provider;
        private readonly Dictionary<Action, EventBinding<StrategicSquadChangedEvent>> _bindings = new();

        public TutorialUnitSearchResolver(ITutorialUnitSlotTargetProvider provider) => _provider = provider;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialUnitSearchRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _provider.TryGetUnitTarget(((TutorialUnitSearchRequest)request).Criteria, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var binding = new EventBinding<StrategicSquadChangedEvent>(_ => callback());
            _bindings[callback] = binding;
            EventBus<StrategicSquadChangedEvent>.Register(binding);
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_bindings.TryGetValue(callback, out var binding)) return;
            EventBus<StrategicSquadChangedEvent>.Deregister(binding);
            _bindings.Remove(callback);
        }
    }
}