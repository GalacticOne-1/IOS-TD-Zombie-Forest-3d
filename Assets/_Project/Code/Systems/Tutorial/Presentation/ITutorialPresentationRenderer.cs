

using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationRenderer
    {
        void RenderHighlights(IReadOnlyList<ITutorialTarget> targets);
        void ClearHighlight();
        void RenderArrow(ITutorialTarget target);
        void ClearArrow();
        void ClearAll();
    }
}
