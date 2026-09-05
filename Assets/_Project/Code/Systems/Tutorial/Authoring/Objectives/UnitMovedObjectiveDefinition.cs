using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>ЗАБЛОКИРОВАН — требует UnitMovedEvent, см. Objectives/UnitMovedObjective.cs.</summary>
    [CreateAssetMenu(fileName = "Objective_UnitMoved",
        menuName = "Game Configs/Tutorial/Objectives/Unit Moved (BLOCKED)")]
    public sealed class UnitMovedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "UnitMoved";
    }
}
