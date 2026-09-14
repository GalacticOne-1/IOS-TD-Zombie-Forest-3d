using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>Критерии поиска юнита в динамическом списке (InventoryManagementController.
    /// VisibleUnits) для guidance-таргетинга. В отличие от ItemId/TutorialTargetId, у юнитов
    /// нет authoring-time стабильного идентификатора (UnitIdentity.Id генерируется в рантайме,
    /// см. DefaultIdentityGenerator) — поэтому таргет всегда резолвится ПОИСКОМ по текущему
    /// состоянию, не lookup'ом по ключу.</summary>
    [Serializable]
    public sealed class TutorialUnitSearchCriteria
    {
        [Tooltip("true = искать юнита НЕ в отряде (свободного); false = искать юнита В отряде.")]
        public bool requireFree = true;

        [Tooltip("TODO: уровень юнита пока не реализован в проекте (нет персонального level " +
                 "у UnitRuntime/UnitDisplayData на момент написания) — поле зарезервировано " +
                 "под будущую фильтрацию, сейчас НЕ используется поиском (см. заглушку в " +
                 "TutorialUnitSlotTargetProvider.Matches).")]
        public bool filterByLevel = false;
        public int requiredLevel = 1;
    }
}