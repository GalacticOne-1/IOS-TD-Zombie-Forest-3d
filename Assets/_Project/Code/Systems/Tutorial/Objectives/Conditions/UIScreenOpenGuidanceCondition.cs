using System;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Экран X (не) открыт". IsSatisfied() всегда читает live-состояние через
    /// ITutorialUIStateQuery — событие только триггерит переоценку, не является источником
    /// истины (см. ITutorialGuidanceCondition докстринг).
    ///
    /// KNOWN LIMITATION: переоценка триггерится только UIScreenOpenedEvent. Если в проекте
    /// появится симметричный UIScreenClosedEvent, стоит добавить вторую подписку сюда же —
    /// без неё expectedOpen=false условия остаются корректными (IsSatisfied() не врёт), но
    /// презентация может не обновиться мгновенно в момент закрытия экрана, а только на
    /// следующее relevant-событие где-то ещё в этом же guidance-списке. Не блокирует
    /// функциональность, задокументировано намеренно (см. аналогичный
    /// TutorialStepDomain "KNOWN LIMITATION" про PostRaidReport).</summary>
    public sealed class UIScreenOpenGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialUIStateQuery _query;
        private readonly UIScreenId _screenId;
        private readonly bool _expectedOpen;
        private EventBinding<UIScreenOpenedEvent> _openBinding;
        private EventBinding<UIScreenClosedEvent> _closeBinding;
        private Action _onMightHaveChanged;

        public UIScreenOpenGuidanceCondition(ITutorialUIStateQuery query, UIScreenId screenId, bool expectedOpen)
        {
            _query = query;
            _screenId = screenId;
            _expectedOpen = expectedOpen;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _openBinding = new EventBinding<UIScreenOpenedEvent>(_ => _onMightHaveChanged?.Invoke());
            _closeBinding = new EventBinding<UIScreenClosedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<UIScreenOpenedEvent>.Register(_openBinding);
            EventBus<UIScreenClosedEvent>.Register(_closeBinding);
        }

        public void Stop()
        {
            if (_openBinding != null)
                EventBus<UIScreenOpenedEvent>.Deregister(_openBinding);
            
            if(_closeBinding != null)
                EventBus<UIScreenClosedEvent>.Deregister(_closeBinding);
            
            _openBinding = null;
            _closeBinding = null;
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied() => _screenId != null && _query.IsScreenOpen(_screenId) == _expectedOpen;
    }
}
