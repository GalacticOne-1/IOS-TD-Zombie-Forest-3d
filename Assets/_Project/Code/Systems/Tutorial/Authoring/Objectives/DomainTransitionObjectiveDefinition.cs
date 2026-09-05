using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>Используется, например, для ReturnedToWorldMap: fromDomain=Raid, toDomain=WorldMap.</summary>
    [CreateAssetMenu(fileName = "Objective_DomainTransition",
        menuName = "Game Configs/Tutorial/Objectives/Domain Transition")]
    public sealed class DomainTransitionObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "DomainTransition";
        public TutorialStepDomain fromDomain;
        public TutorialStepDomain toDomain;
    }
}
