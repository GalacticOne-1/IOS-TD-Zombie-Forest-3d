using UnityEngine;
using Galactic1.Code.Systems.Construction.Configs;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "вкладка категории X (не) выбрана в панели строительства" —
    /// та часть состояния, которую FacilityPanelOpenGuidanceConditionDefinition намеренно
    /// не покрывает (панель может быть открыта с любой вкладкой). Обычно комбинируется
    /// через AllOf с FacilityPanelOpen: "панель открыта AND нужная вкладка ещё НЕ выбрана" —
    /// именно эта комбинация гасит highlight после клика по вкладке (см. её докстринг —
    /// без него highlight не реагирует на клик, только на закрытие/открытие панели).</summary>
    [CreateAssetMenu(fileName = "Guidance_ConstructionTabSelected",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Construction Tab Selected")]
    public sealed class ConstructionTabSelectedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "ConstructionTabSelected";

        public ConstructionCategory category;

        [Tooltip("true = условие истинно, когда вкладка ЭТОЙ категории ВЫБРАНА; " +
                 "false = когда НЕ выбрана (эта форма и держит highlight до клика).")]
        public bool expectedSelected = true;
    }
}