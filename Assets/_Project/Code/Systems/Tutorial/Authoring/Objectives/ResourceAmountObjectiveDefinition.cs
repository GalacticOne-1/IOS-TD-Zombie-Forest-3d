using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ResourceAmount",
        menuName = "Game Configs/Tutorial/Objectives/Resource Amount (Camp Storage)")]
    public sealed class ResourceAmountObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ResourceAmount";
        public ItemId itemId;
        [Min(1)] public int requiredAmount = 5;
    }
}
