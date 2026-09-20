using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Runtime POCO-снэпшот "что показать прямо сейчас", строится заново на
    /// каждый вызов TutorialService.BuildEffectivePresentation. HighlightRequest уже
    /// сконвертирован через TutorialTargetRequestFactory — TutorialPresentationService
    /// никогда не видит TutorialTargetQuery/[SerializeReference]-граф.</summary>
    public sealed class TutorialEffectivePresentation
    {
        public string InstructionTitleKey;
        public string InstructionDesKey;
        public string DialogueId;
        public TutorialInputMode InputPolicy;
        public IReadOnlyList<TutorialTargetRequest> HighlightRequests { get; set; }
        public TutorialTargetId ArrowTargetId;
        public TutorialTargetId CameraFocusTargetId;
    }
}