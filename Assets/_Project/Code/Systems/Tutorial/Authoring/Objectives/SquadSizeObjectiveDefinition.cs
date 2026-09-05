using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_SquadSize",
        menuName = "Game Configs/Tutorial/Objectives/Squad Size")]
    public sealed class SquadSizeObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "SquadSize";
        [Min(1)] public int requiredSize = 4;
    }
}
