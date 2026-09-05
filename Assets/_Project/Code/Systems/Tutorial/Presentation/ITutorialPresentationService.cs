using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationService
    {
        void Show(TutorialPresentationDefinition presentation);
        void Hide();
    }

    /// <summary>Заглушка на случай, если presentation-слой временно отключён/тестируется
    /// изолированно — TutorialService всегда получает нечто валидное.</summary>
    public sealed class NullTutorialPresentationService : ITutorialPresentationService
    {
        public void Show(TutorialPresentationDefinition presentation) { }
        public void Hide() { }
    }
}
