// ConstructionGhostActiveGuidanceConditionDefinition.cs
using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие "ghost здания этой конфигурации (не) существует на сцене сейчас".
    /// Комбинируется через AllOf с FacilityPanelOpen для гашения highlight карточки после
    /// клика: AllOf(FacilityPanelOpen(true), GhostActive(facilityItemId=X, expectedActive=
    /// false)) — панель открыта AND ghost этого здания ещё НЕ создан. Обратный вариант
    /// (GhostActive(expectedActive=true)) — для следующего guidance-шага (например
    /// Confirm-кнопки), активного, пока ghost существует, и гаснущего при Cancel.</summary>
    [CreateAssetMenu(fileName = "Guidance_ConstructionGhostActive",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Construction Ghost Active")]
    public sealed class ConstructionGhostActiveGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "ConstructionGhostActive";

        public ItemId facilityItemId;

        [Tooltip("true = условие истинно, когда ghost ЭТОГО здания СУЩЕСТВУЕТ; " +
                 "false = когда НЕ существует (эта форма держит highlight карточки до клика).")]
        public bool expectedActive = true;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (facilityItemId == null)
            {
                error = "ConstructionGhostActiveGuidanceConditionDefinition: facilityItemId is empty.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}