
using System;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class InventoryMainTabSelectedGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialInventoryMainTabQuery _query;
        private readonly InventoryGameplayMode _mode;
        private readonly bool _expectedSelected;
        private Action _onMightHaveChanged;

        public InventoryMainTabSelectedGuidanceCondition(
            ITutorialInventoryMainTabQuery query, InventoryGameplayMode mode, bool expectedSelected)
        {
            _query = query;
            _mode = mode;
            _expectedSelected = expectedSelected;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnMainTabSelected += OnTabSelected;
        }

        public void Stop()
        {
            _query.OnMainTabSelected -= OnTabSelected;
            _onMightHaveChanged = null;
        }

        private void OnTabSelected(InventoryGameplayMode selected) => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => (_query.CurrentMainTab == _mode) == _expectedSelected;
    }
}