namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public interface ITutorialPresentationService
    {
        void Show(TutorialEffectivePresentation presentation);

        /// <summary>Полный cleanup: визуалы + camera constraint. Использовать при
        /// действительной остановке presentation lifecycle (StopTutorial) — НЕ в обычном
        /// completion-transition (там HideVisuals(), см. её докстринг в реализации).</summary>
        void Hide();

        /// <summary>Скрывает только tutorial visuals (highlight/arrow/highlight-list),
        /// сохраняя текущий camera constraint. Используется в окне между завершением шага
        /// и фактической активацией следующего (completion delay) — камера не должна
        /// становиться unrestricted раньше времени.</summary>
        void HideVisuals();

        /// <summary>Снимает camera constraint независимо от визуалов. Использовать там, где
        /// Show() следующего шага не последует вовсе (терминальное завершение кампании).</summary>
        void ClearCameraConstraint();
    }

    public sealed class NullTutorialPresentationService : ITutorialPresentationService
    {
        public void Show(TutorialEffectivePresentation presentation) { }
        public void Hide() { }
        public void HideVisuals() { }
        public void ClearCameraConstraint() { }
    }
}