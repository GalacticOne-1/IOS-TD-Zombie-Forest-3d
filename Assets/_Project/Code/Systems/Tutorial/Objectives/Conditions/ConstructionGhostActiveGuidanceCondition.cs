
using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ConstructionGhostActiveGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialConstructionGhostQuery _query;
        private readonly ItemId _facilityItemId;
        private readonly bool _expectedActive;
        private Action _onMightHaveChanged;

        public ConstructionGhostActiveGuidanceCondition(
            ITutorialConstructionGhostQuery query, ItemId facilityItemId, bool expectedActive)
        {
            _query = query;
            _facilityItemId = facilityItemId;
            _expectedActive = expectedActive;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnGhostChanged += OnGhostChanged;
        }

        public void Stop()
        {
            _query.OnGhostChanged -= OnGhostChanged;
            _onMightHaveChanged = null;
        }

        private void OnGhostChanged(ItemId changedItemId) => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => (_query.CurrentGhostFacilityItemId == _facilityItemId) == _expectedActive;
    }
}