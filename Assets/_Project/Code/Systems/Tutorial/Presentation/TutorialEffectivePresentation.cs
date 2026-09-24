using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public sealed class TutorialEffectivePresentation
    {
        public string InstructionTitleKey;
        public string InstructionDesKey;
        public string DialogueId;
        public TutorialInputMode InputPolicy;
        public IReadOnlyList<TutorialTargetRequest> HighlightRequests { get; set; }
        public TutorialTargetId ArrowTargetId;
        public TutorialTargetId CameraFocusTargetId;

        /// <summary>ADDED — раздел 10 ТЗ camera bounds. Null == нет constraint (тот же
        /// смысл, что TutorialCameraConstraint.None) — TutorialPresentationService трактует
        /// оба варианта одинаково.</summary>
        public TutorialCameraConstraint CameraConstraint { get; set; }
    }
}