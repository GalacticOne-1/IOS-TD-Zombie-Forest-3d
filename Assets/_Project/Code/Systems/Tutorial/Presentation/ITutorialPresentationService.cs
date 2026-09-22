
namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationService
    {
        void Show(TutorialEffectivePresentation presentation);
        void Hide();
    }

    public sealed class NullTutorialPresentationService : ITutorialPresentationService
    {
        public void Show(TutorialEffectivePresentation presentation)
        {
        }

        public void Hide()
        {
        }
    }
}