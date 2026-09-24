using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Scene-authored прямоугольная XZ-область для tutorial camera constraint.
    /// Unity Bounds намеренно допустим здесь (не в Runtime/Presentation-POCO слое) — этот
    /// интерфейс живёт на той же границе, что ITutorialTarget/TutorialTargetRegistry:
    /// абстракция над сценой, не над gameplay-состоянием.</summary>
    public interface ITutorialCameraBoundsTarget
    {
        TutorialTargetId TargetId { get; }
        Bounds WorldBounds { get; }
    }
}