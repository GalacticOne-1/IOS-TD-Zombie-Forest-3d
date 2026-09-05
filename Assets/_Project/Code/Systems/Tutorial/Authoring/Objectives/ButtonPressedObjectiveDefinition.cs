
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ButtonPressed", 
        menuName = "Game Configs/Tutorial/Objectives/Button Pressed")]
    public sealed class ButtonPressedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ButtonPressed";
        [Tooltip("Совпадает с targetId будущего ITutorialTarget/UITargetInteractedEvent.")]
        public TutorialTargetId targetId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (targetId == null) { error = "ButtonPressedObjectiveDefinition: targetId is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}
