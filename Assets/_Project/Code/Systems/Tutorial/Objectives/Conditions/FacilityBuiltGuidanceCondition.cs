using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Objectives;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Рантайм guidance-условие "здание X (не) построено" — тот же контракт
    /// (Start/Stop/IsSatisfied), что у остальных ITutorialGuidanceCondition реализаций
    /// (см. ItemEquippedGuidanceCondition/FacilityPanelOpenGuidanceCondition). Подписка на
    /// ITutorialConstructionQuery.OnFacilityBuilt — только сигнал "перепроверь", источник
    /// истины всегда HasFacilityBuilt().</summary>
    public sealed class FacilityBuiltGuidanceCondition : ITutorialGuidanceCondition
    {
        private readonly ITutorialConstructionQuery _query;
        private readonly ItemId _itemId;
        private readonly bool _expectedBuilt;
        private Action _onMightHaveChanged;

        public FacilityBuiltGuidanceCondition(ITutorialConstructionQuery query, ItemId itemId, bool expectedBuilt)
        {
            _query = query;
            _itemId = itemId;
            _expectedBuilt = expectedBuilt;
        }

        public void Start(Action onMightHaveChanged)
        {
            _onMightHaveChanged = onMightHaveChanged;
            _query.OnFacilityBuilt += OnFacilityBuilt;
        }

        public void Stop() => _query.OnFacilityBuilt -= OnFacilityBuilt;

        private void OnFacilityBuilt(ItemId builtItemId) => _onMightHaveChanged?.Invoke();

        public bool IsSatisfied() => _query.HasFacilityBuilt(_itemId) == _expectedBuilt;
    }
}
