using System;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class ConstructionTabSelectedGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialConstructionTabQuery _query;
        private readonly ConstructionCategory _category;
        private readonly bool _expectedSelected;
        private Action _onMightHaveChanged;

        public ConstructionTabSelectedGuidanceCondition(
            ITutorialConstructionTabQuery query, ConstructionCategory category, bool expectedSelected)
        {
            _query = query;
            _category = category;
            _expectedSelected = expectedSelected;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnConstructionTabSelected += OnTabSelected;
        }

        public void Stop()
        {
            _query.OnConstructionTabSelected -= OnTabSelected;
            _onMightHaveChanged = null;
        }

        private void OnTabSelected(ConstructionCategory selected) => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => (_query.CurrentTabCategory == _category) == _expectedSelected;
    }
}