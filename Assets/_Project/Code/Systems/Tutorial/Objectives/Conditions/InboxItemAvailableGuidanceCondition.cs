
using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class InboxItemAvailableGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialInboxQuery _query;
        private readonly ItemId _itemId;
        private readonly bool _expectedAvailable;
        private Action _onMightHaveChanged;

        public InboxItemAvailableGuidanceCondition(
            ITutorialInboxQuery query, ItemId itemId, bool expectedAvailable)
        {
            _query = query;
            _itemId = itemId;
            _expectedAvailable = expectedAvailable;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnInboxChanged += OnChanged;
        }

        public void Stop()
        {
            _query.OnInboxChanged -= OnChanged;
            _onMightHaveChanged = null;
        }

        private void OnChanged() => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => _query.HasItemInInbox(_itemId) == _expectedAvailable;
    }
}