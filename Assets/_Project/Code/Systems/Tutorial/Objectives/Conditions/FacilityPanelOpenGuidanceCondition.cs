using System;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class FacilityPanelOpenGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialFacilityPanelQuery _query;
        private readonly FacilityType _facilityType;
        private readonly bool _expectedOpen;
        private EventBinding<FacilityPanelOpenedEvent> _openBinding;
        private EventBinding<UIScreenClosedEvent> _closeBinding;
        private Action _onMightHaveChanged;

        public FacilityPanelOpenGuidanceCondition(
            ITutorialFacilityPanelQuery query, FacilityType facilityType, bool expectedOpen)
        {
            _query = query;
            _facilityType = facilityType;
            _expectedOpen = expectedOpen;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _openBinding = new EventBinding<FacilityPanelOpenedEvent>(_ => _onMightHaveChanged?.Invoke());
            _closeBinding = new EventBinding<UIScreenClosedEvent>(_ => _onMightHaveChanged?.Invoke());
            EventBus<FacilityPanelOpenedEvent>.Register(_openBinding);
            EventBus<UIScreenClosedEvent>.Register(_closeBinding);
        }

        public void Stop()
        {
            if (_openBinding != null) EventBus<FacilityPanelOpenedEvent>.Deregister(_openBinding);
            if (_closeBinding != null) EventBus<UIScreenClosedEvent>.Deregister(_closeBinding);
            _openBinding = null;
            _closeBinding = null;
            _onMightHaveChanged = null;
        }

        public bool IsSatisfied() => _query.IsFacilityPanelOpen(_facilityType) == _expectedOpen;
    }
}