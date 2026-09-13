using UnityEngine;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "открыта FacilityPanel для типа здания X". В отличие от
    /// UIScreenOpenGuidanceConditionDefinition — там ScreenId однозначно определяет
    /// экран, здесь ScreenId один (FacilityPanel) для всех типов, различие даёт
    /// FacilityType (см. ITutorialFacilityPanelQuery докстринг).</summary>
    [CreateAssetMenu(fileName = "Guidance_FacilityPanelOpen",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Facility Panel Open")]
    public sealed class FacilityPanelOpenGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "FacilityPanelOpen";

        public FacilityType facilityType;

        [Tooltip("true = условие истинно, когда панель ЭТОГО типа ОТКРЫТА; false = когда НЕ открыта.")]
        public bool expectedOpen = true;
    }
}