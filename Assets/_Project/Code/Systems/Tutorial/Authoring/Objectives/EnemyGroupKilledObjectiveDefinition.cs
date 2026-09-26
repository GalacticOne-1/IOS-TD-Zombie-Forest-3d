
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>В отличие от EnemyKilledObjectiveDefinition (любые N смертей на сцене),
    /// трекает конкретный, зафиксированный один раз набор врагов — ближайших count
    /// к originTargetId в момент старта объектива.</summary>
    [CreateAssetMenu(fileName = "Objective_EnemyGroupKilled",
        menuName = "Game Configs/Tutorial/Objectives/Enemy Group Killed")]
    public sealed class EnemyGroupKilledObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "EnemyGroupKilled";

        [Tooltip("Ориентир рядом с группой врагов (например точка спавна) — сам не является врагом.")]
        public TutorialTargetId originTargetId;

        [Min(1)] public int count = 1;
    }
}