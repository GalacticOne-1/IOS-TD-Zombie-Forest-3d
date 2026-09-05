using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ContainerOpened", 
        menuName = "Game Configs/Tutorial/Objectives/Container Opened")]
    public sealed class ContainerOpenedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ContainerOpened";
    }
}
