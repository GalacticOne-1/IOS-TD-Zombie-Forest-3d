using System;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class UnitSelectGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialSquadQuery _squad;
        private readonly bool _expectedFree;
        private EventBinding<SurvivorSelectedEvent> _selectBinding;
        private EventBinding<StrategicSquadChangedEvent> _squadChangedBinding;
        private Action _onMightHaveChanged;

        public UnitSelectGuidanceCondition(ITutorialSquadQuery squad, bool expectedFree)
        {
            _squad = squad;
            _expectedFree = expectedFree;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;

            _selectBinding = new EventBinding<SurvivorSelectedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<SurvivorSelectedEvent>.Register(_selectBinding);

            _squadChangedBinding = new EventBinding<StrategicSquadChangedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<StrategicSquadChangedEvent>.Register(_squadChangedBinding);
        }

        public void Stop()
        {
            if (_selectBinding != null)
                EventBus<SurvivorSelectedEvent>.Deregister(_selectBinding);
            if (_squadChangedBinding != null)
                EventBus<StrategicSquadChangedEvent>.Deregister(_squadChangedBinding);
            _selectBinding = null;
            _squadChangedBinding = null;
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied()
            => _squad.SurvivorIsFree() == _expectedFree;
    }
}