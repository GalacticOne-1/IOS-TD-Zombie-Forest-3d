using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    [CreateAssetMenu(fileName = "Guidance_Unit Select",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Unit Select")]
    public sealed class UnitSelectGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "UnitSelect";


        [Tooltip("true = условие истинно, когда выбран СВОБОДНЫЙ юнит; false = когда В ОТРЯДЕ.")]
        public bool expectedFree = true;

    }
}