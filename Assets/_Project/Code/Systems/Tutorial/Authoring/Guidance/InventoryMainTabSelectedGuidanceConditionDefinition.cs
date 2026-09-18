
using Galactic1.Code.Inventory.Context;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие "главная вкладка инвентаря (не) выбрана" — см.
    /// ITutorialInventoryMainTabQuery докстринг про раздельные каналы при общем
    /// InventoryGameplayMode. Обычно комбинируется через AllOf с UIScreenOpen(Inventory) —
    /// тот же паттерн, что ConstructionTabSelected с FacilityPanelOpen: "экран открыт AND
    /// вкладка ещё НЕ выбрана" гасит highlight сразу после клика.</summary>
    [CreateAssetMenu(fileName = "Guidance_InventoryMainTabSelected",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Inventory Main Tab Selected")]
    public sealed class InventoryMainTabSelectedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "InventoryMainTabSelected";

        public InventoryGameplayMode mode;

        [Tooltip("true = условие истинно, когда эта вкладка ВЫБРАНА; false = когда НЕ " +
                 "выбрана (эта форма держит highlight до клика).")]
        public bool expectedSelected = true;
    }
}