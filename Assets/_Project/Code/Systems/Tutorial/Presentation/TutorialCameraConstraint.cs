using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Runtime-safe снэпшот camera spatial constraint для одного шага — несёт
    /// только Mode + TutorialTargetId, НЕ резолвленные world-координаты. Та же граница,
    /// что уже используют ArrowTargetId/CameraFocusTargetId в TutorialEffectivePresentation:
    /// сам presentation-слой не резолвит Unity Bounds, это делает TutorialPresentationService
    /// через TutorialCameraBoundsRegistry в момент Render(). Живёт в Presentation namespace,
    /// не Runtime — та же организация, что у соседнего TutorialTargetRequest: POCO без
    /// UnityEngine-зависимостей, но логически часть presentation-цепочки, не
    /// objectives/conditions gameplay-логики.</summary>
    public sealed class TutorialCameraConstraint
    {
        public readonly TutorialCameraConstraintMode Mode;
        public readonly TutorialTargetId BoundsTargetId;

        public TutorialCameraConstraint(TutorialCameraConstraintMode mode, TutorialTargetId boundsTargetId)
        {
            Mode = mode;
            BoundsTargetId = boundsTargetId;
        }

        public static readonly TutorialCameraConstraint None =
            new(TutorialCameraConstraintMode.None, null);
    }
}