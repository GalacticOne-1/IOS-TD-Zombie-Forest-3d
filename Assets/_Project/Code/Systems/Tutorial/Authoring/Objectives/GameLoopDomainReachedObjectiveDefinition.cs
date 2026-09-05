using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_GameLoopDomainReached",
        menuName = "Game Configs/Tutorial/Objectives/Game Loop Domain Reached")]
    public sealed class GameLoopDomainReachedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "GameLoopDomainReached";
        public TutorialStepDomain targetDomain;
    }
}
