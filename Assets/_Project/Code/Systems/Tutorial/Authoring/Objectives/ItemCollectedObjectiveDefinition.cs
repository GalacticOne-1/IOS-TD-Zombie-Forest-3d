using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ItemCollected", 
        menuName = "Game Configs/Tutorial/Objectives/Item Collected (Event)")]
    public sealed class ItemCollectedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ItemCollected";
        public ItemId itemId;
        [Min(1)] public int requiredAmount = 1;
    }
}
