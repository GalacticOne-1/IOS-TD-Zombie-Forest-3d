using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Базовая реализация для state-СЕМАНТИЧЕСКИХ объективов: событие — лишь сигнал
    /// "что-то изменилось, перепроверь состояние", payload события никогда не используется
    /// как критерий завершения напрямую (в отличие от TutorialEventObjectiveBase).
    /// Пример: "у игрока есть >= 5 еды" — CampStorageChangedEvent лишь триггерит повторную
    /// проверку GetCampStorageAmount(), сам факт события не значит "еды теперь достаточно".
    /// </summary>
    public abstract class TutorialStateRecheckObjectiveBase<TChangeEvent> : ITutorialObjective
        where TChangeEvent : IEvent
    {
        private EventBinding<TChangeEvent> _binding;
        private System.Action _onProgressChanged;
        public bool IsCompleted { get; private set; }

        public void Start(System.Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;

            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
                return;
            }

            _binding = new EventBinding<TChangeEvent>(OnChanged);
            EventBus<TChangeEvent>.Register(_binding);
        }

        public void Stop()
        {
            if (_binding != null)
                EventBus<TChangeEvent>.Deregister(_binding);
            _binding = null;
            _onProgressChanged = null;
        }

        private void OnChanged(TChangeEvent e)
        {
            if (IsCompleted) return;
            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
            }
        }

        public abstract bool EvaluateCurrentState();
        public bool EvaluateEvent(object gameplayEvent) => false;
    }
}
