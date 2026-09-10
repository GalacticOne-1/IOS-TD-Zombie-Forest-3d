

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationRenderer
    {
        void RenderHighlight(ITutorialTarget target);
        void ClearHighlight();
        void RenderArrow(ITutorialTarget target);
        void ClearArrow();
        void ClearAll();
    }
}
