using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>ЗАБЛОКИРОВАН — требует WeaponFiredEvent, см. Objectives/WeaponFiredObjective.cs.</summary>
    [CreateAssetMenu(fileName = "Objective_WeaponFired", 
        menuName = "Game Configs/Tutorial/Objectives/Weapon Fired (BLOCKED)")]
    public sealed class WeaponFiredObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "WeaponFired";
    }
}
