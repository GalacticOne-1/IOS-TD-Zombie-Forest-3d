using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Runtime-снэпшот "что показать" для одного guidance-варианта. Ровно три
    /// поля — HighlightRequest уже сконвертирован через TutorialTargetRequestFactory
    /// (см. TutorialService.BuildGuidanceEntries), Arrow/Camera остаются TutorialTargetId,
    /// резолвятся через тот же TutorialTargetRegistry, что и раньше (не мигрируют на
    /// query-систему — см. ТЗ п.12).</summary>
    public sealed class TutorialGuidanceTarget
    {
        public readonly TutorialTargetRequest HighlightRequest;
        public readonly TutorialTargetId ArrowTargetId;
        public readonly TutorialTargetId CameraFocusTargetId;

        public TutorialGuidanceTarget(
            TutorialTargetRequest highlightRequest,
            TutorialTargetId arrowTargetId,
            TutorialTargetId cameraFocusTargetId)
        {
            HighlightRequest = highlightRequest;
            ArrowTargetId = arrowTargetId;
            CameraFocusTargetId = cameraFocusTargetId;
        }
    }
}