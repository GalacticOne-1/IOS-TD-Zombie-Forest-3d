using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_InboxItemCollected", 
        menuName = "Game Configs/Tutorial/Objectives/Inbox Item Collected (Event)")]
    public sealed class InboxItemCollectedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "InboxItemCollected";
        public ItemId itemId;
    }
}