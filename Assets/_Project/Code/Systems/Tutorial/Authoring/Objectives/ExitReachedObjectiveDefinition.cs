using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ExitReached", 
        menuName = "Game Configs/Tutorial/Objectives/Exit Reached")]
    public sealed class ExitReachedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ExitReached";
    }
}
