using System;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Runtime для AbilitySlotsOpenGuidanceConditionDefinition. _isOpen — живое
    /// состояние, переключается обоими событиями (не "разово открывался"), поэтому highlight
    /// корректно возвращается при повторном закрытии списка — тот же паттерн, что у
    /// UIScreenOpenGuidanceCondition.</summary>
    public sealed class AbilitySlotsOpenGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly bool _expectedOpen;
        private EventBinding<AbilitySlotsOpenedEvent> _openedBinding;
        private EventBinding<AbilitySlotsClosedEvent> _closedBinding;
        private Action _onChanged;
        private bool _isOpen;

        public AbilitySlotsOpenGuidanceCondition(bool expectedOpen)
        {
            _expectedOpen = expectedOpen;
        }

        public void Start(Action onChanged)
        {
            _onChanged = onChanged;
            _isOpen = false; // карточка открывается только явным действием игрока — стартуем с "закрыт"

            _openedBinding = new EventBinding<AbilitySlotsOpenedEvent>(OnOpened);
            EventBus<AbilitySlotsOpenedEvent>.Register(_openedBinding);

            _closedBinding = new EventBinding<AbilitySlotsClosedEvent>(OnClosed);
            EventBus<AbilitySlotsClosedEvent>.Register(_closedBinding);
        }

        public void Stop()
        {
            if (_openedBinding != null)
                EventBus<AbilitySlotsOpenedEvent>.Deregister(_openedBinding);
            _openedBinding = null;

            if (_closedBinding != null)
                EventBus<AbilitySlotsClosedEvent>.Deregister(_closedBinding);
            _closedBinding = null;

            _onChanged = null;
        }

        private void OnOpened(AbilitySlotsOpenedEvent e) => SetOpen(true);
        private void OnClosed(AbilitySlotsClosedEvent e) => SetOpen(false);

        private void SetOpen(bool value)
        {
            if (_isOpen == value) return;
            _isOpen = value;
            _onChanged?.Invoke();
        }

        public bool IsSatisfied() => _isOpen == _expectedOpen;
    }
}