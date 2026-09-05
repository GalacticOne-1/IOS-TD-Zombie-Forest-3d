using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_RaidCompleted",
        menuName = "Game Configs/Tutorial/Objectives/Raid Completed")]
    public sealed class RaidCompletedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "RaidCompleted";
        [Tooltip("Если true — засчитывается только победа. Если false — любое завершение рейда.")]
        public bool requireVictory = true;
    }
}
