using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>ЗАБЛОКИРОВАН — требует TargetSelectedEvent, см. Objectives/TargetSelectedObjective.cs.</summary>
    [CreateAssetMenu(fileName = "Objective_TargetSelected", 
        menuName = "Game Configs/Tutorial/Objectives/Target Selected (BLOCKED)")]
    public sealed class TargetSelectedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "TargetSelected";
    }
}
