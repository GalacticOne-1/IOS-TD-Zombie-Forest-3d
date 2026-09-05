using UnityEngine;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_UIScreenOpened", 
        menuName = "Game Configs/Tutorial/Objectives/UI Screen Opened")]
    public sealed class UIScreenOpenedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "UIScreenOpened";
        public UIScreenId screenId;
    }
}
