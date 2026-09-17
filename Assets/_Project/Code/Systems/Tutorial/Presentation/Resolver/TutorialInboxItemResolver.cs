
using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>ВНИМАНИЕ: retrigger-событие — FacilityPanelOpenedEvent, не что-то
    /// inbox-специфичное. Так было и в старом Render() (см. SubscribeInboxHighlightRetrigger)
    /// — перенесено дословно, не исправлялось: похоже на существовавший баг/недосмотр,
    /// вне scope этого рефакторинга. Тот же провайдер-интерфейс, что у инвентаря
    /// (ITutorialItemSlotTargetProvider) — просто другой инстанс (TaskInboxSlotTargetProvider).</summary>
    public sealed class TutorialInboxItemResolver : ITutorialTargetResolver
    {
        private readonly ITutorialItemSlotTargetProvider _provider;
        private readonly Dictionary<Action, EventBinding<FacilityPanelOpenedEvent>> _bindings = new();

        public TutorialInboxItemResolver(ITutorialItemSlotTargetProvider provider) => _provider = provider;

        public bool CanResolve(TutorialTargetRequest request) => request is TutorialInboxItemRequest;

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
            => _provider.TryGetSlotTarget(((TutorialInboxItemRequest)request).ItemId, out target);

        public void SubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            var binding = new EventBinding<FacilityPanelOpenedEvent>(_ => callback());
            _bindings[callback] = binding;
            EventBus<FacilityPanelOpenedEvent>.Register(binding);
        }

        public void UnsubscribeInvalidation(TutorialTargetRequest request, Action callback)
        {
            if (!_bindings.TryGetValue(callback, out var binding)) return;
            EventBus<FacilityPanelOpenedEvent>.Deregister(binding);
            _bindings.Remove(callback);
        }
    }
}