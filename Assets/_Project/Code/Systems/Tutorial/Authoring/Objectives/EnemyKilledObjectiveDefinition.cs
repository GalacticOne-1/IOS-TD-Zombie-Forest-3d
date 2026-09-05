using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_EnemyKilled",
        menuName = "Game Configs/Tutorial/Objectives/Enemy Killed")]
    public sealed class EnemyKilledObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "EnemyKilled";
        [Min(1)] public int requiredCount = 1;
    }
}
