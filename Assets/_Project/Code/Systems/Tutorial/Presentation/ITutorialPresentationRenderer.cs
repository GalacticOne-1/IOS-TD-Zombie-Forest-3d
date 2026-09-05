namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationRenderer
    {
        void RenderInstruction(string textKey);
        void ClearInstruction();
        void RenderHighlight(ITutorialTarget target);
        void ClearHighlight();
        void RenderArrow(ITutorialTarget target);
        void ClearArrow();
        void ClearAll();
    }
}
