using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// Generic objective "отряд дошёл до мировой точки" — раздел 4/8 ТЗ Chapter 2.
    /// Не управляет движением (SquadMovementSystem.IssueMove не вызывается отсюда) —
    /// только наблюдает факт прибытия. targetPosition/radius — обычные Unity-serializable
    /// поля: этот класс Authoring/ScriptableObject-слой, Unity-зависимость здесь
    /// допустима и ожидаема (в отличие от Runtime-объектива).
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_MoveToLocation",
        menuName = "Game Configs/Tutorial/Objectives/Move To Location")]
    public sealed class MoveToLocationObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "MoveToLocation";

        [Tooltip("Целевая мировая позиция, до которой должен дойти отряд (центр масс).")]
        public Vector3 targetPosition;

        [Min(0.1f)]
        [Tooltip("Допустимое расстояние от targetPosition, при котором объектив считается выполненным.")]
        public float radius = 2f;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (radius <= 0f)
            {
                error = "MoveToLocationObjectiveDefinition: radius must be > 0.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}
