
using Galactic1.Code.Inventory.Context;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    [CreateAssetMenu(fileName = "Guidance_InventorySquadExtraTabSelected",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Inventory Squad Extra Tab Selected")]
    public sealed class InventorySquadExtraTabSelectedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "InventorySquadExtraTabSelected";

        public InventoryGameplayMode mode;

        [Tooltip("true = условие истинно, когда эта вкладка ВЫБРАНА; false = когда НЕ выбрана.")]
        public bool expectedSelected = true;
    }
}