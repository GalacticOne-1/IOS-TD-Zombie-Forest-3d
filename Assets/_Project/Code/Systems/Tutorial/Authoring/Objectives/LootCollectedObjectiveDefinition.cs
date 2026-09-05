using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_LootCollected", 
        menuName = "Game Configs/Tutorial/Objectives/Loot Collected")]
    public sealed class LootCollectedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "LootCollected";
    }
}
