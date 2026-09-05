using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Базовая реализация для event-СЕМАНТИЧЕСКИХ объективов: сам факт события ЯВЛЯЕТСЯ
    /// критерием завершения (например EnemyKilledEvent → "враг убит"). Инкапсулирует
    /// EventBus-подписку/отписку по образцу MissionObjectiveService.
    ///
    /// Отличать от TutorialStateRecheckObjectiveBase — там событие лишь сигнал
    /// "перепроверь состояние", а не сам критерий (см. ResourceAmountObjective/SquadSizeObjective).
    /// </summary>
    public abstract class TutorialEventObjectiveBase<TEvent> : ITutorialObjective
        where TEvent : IEvent
    {
        private EventBinding<TEvent> _binding;
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

            _binding = new EventBinding<TEvent>(OnEvent);
            EventBus<TEvent>.Register(_binding);
        }

        public void Stop()
        {
            if (_binding != null)
                EventBus<TEvent>.Deregister(_binding);
            _binding = null;
            _onProgressChanged = null;
        }

        private void OnEvent(TEvent e)
        {
            if (IsCompleted) return;
            if (EvaluateEvent(e))
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
            }
        }

        /// <summary>По умолчанию нет ретроактивного завершения — большинство event-объективов
        /// не имеют смысла "уже произошло" (например EnemyKilled не проверяется на Start()).
        /// Переопределяется там, где ретроактивность осмысленна (ItemEquippedObjective).</summary>
        public virtual bool EvaluateCurrentState() => false;

        bool ITutorialObjective.EvaluateEvent(object gameplayEvent)
            => gameplayEvent is TEvent typed && EvaluateEvent(typed);

        protected abstract bool EvaluateEvent(TEvent e);
    }
}
