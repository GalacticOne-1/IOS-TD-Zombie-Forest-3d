
using System;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ZoneEnteredObjective : ITutorialObjective
    {
        private readonly TutorialTargetId _targetId;
        private readonly ITutorialObjective _inner; // null = вход в зону сам завершает объектив

        private Action _onProgressChanged;
        private EventBinding<TutorialZoneEnteredEvent> _binding;
        private bool _entered;
        private bool _innerStarted;
        private bool _awaiting;

        public ZoneEnteredObjective(
            TutorialTargetId targetId,
            TutorialObjectiveDefinition innerDef,
            Func<TutorialObjectiveDefinition, ITutorialObjective> create)
        {
            _targetId = targetId;
            _inner = innerDef != null ? create(innerDef) : null;
        }

        public bool IsCompleted => _entered && (_inner == null || _inner.IsCompleted);

        public void Start(Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;
            _binding = new EventBinding<TutorialZoneEnteredEvent>(OnZoneEntered);
            EventBus<TutorialZoneEnteredEvent>.Register(_binding);

            _awaiting = true;
            TutorialZoneTracker.Add(_targetId);
        }

        public void Stop()
        {
            StopAwaiting();
            if (_innerStarted) _inner.Stop();
            _innerStarted = false;
            _onProgressChanged = null;
        }

        private void StopAwaiting()
        {
            if (_binding != null)
                EventBus<TutorialZoneEnteredEvent>.Deregister(_binding);
            _binding = null;

            if (_awaiting)
            {
                _awaiting = false;
                TutorialZoneTracker.Remove(_targetId);
            }
        }

        private void OnZoneEntered(TutorialZoneEnteredEvent e)
        {
            if (_entered || e.TargetId != _targetId) return;

            StopAwaiting();
            _entered = true;

            if (_inner != null)
            {
                _innerStarted = true;
                _inner.Start(() => _onProgressChanged?.Invoke());
            }

            _onProgressChanged?.Invoke(); // при _inner == null шаг увидит IsCompleted и завершится
        }

        public bool EvaluateCurrentState() => false;

        public bool EvaluateEvent(object gameplayEvent)
            => _innerStarted && _inner.EvaluateEvent(gameplayEvent);

        public bool TryGetProgress(out int current, out int required)
        {
            if (_inner != null) return _inner.TryGetProgress(out current, out required);
            current = 0;
            required = 0;
            return false;
        }
    }
}