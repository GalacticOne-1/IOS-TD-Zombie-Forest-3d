using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Scene-local рендерер тутор-презентации. Обычный UIScreenPanel под
    /// _layerRoot.hudRoot (аналогично HUDInput/HUDCamp), пересоздаётся при каждой
    /// загрузке сцены — presentation discovers active step, а не наоборот.
    ///
    /// НЕ IGameService — единственная точка обнаружения этого класса это
    /// AttachRenderer/DetachRenderer через TutorialPresentationService, регистрация
    /// в ServiceLocator была мёртвым весом (ничто её не читало) и убрана.
    ///
    /// ⚠️ Требует UIScreenId.TutorialHUD в enum + добавления в UIScreenManager.GetRoot
    /// и PreloadScreens (см. Integration/).
    /// </summary>
    public sealed class TutorialHUDController : UIScreenPanel, ITutorialPresentationRenderer
    {
        [SerializeField] private TutorialInstructionView instructionView;
        [SerializeField] private RectTransform highlightLayer;
        [SerializeField] private RectTransform arrowLayer;
        [SerializeField] private TutorialHighlightWidget highlightPrefab;
        [SerializeField] private TutorialArrowWidget arrowPrefab;

        private TutorialHighlightWidget _activeHighlight;
        private TutorialArrowWidget _activeArrow;

        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Get<TutorialPresentationService>().AttachRenderer(this);
        }

        public override void Remove()
        {
            ServiceLocator.Current.Get<TutorialPresentationService>().DetachRenderer(this);
        }

        public void RenderInstruction(string textKey) => instructionView.Show(textKey);
        public void ClearInstruction() => instructionView.Hide();

        public void RenderHighlight(ITutorialTarget target)
        {
            ClearHighlight();
            if (target?.UIAnchor == null) return;
            _activeHighlight = Instantiate(highlightPrefab, highlightLayer);
            _activeHighlight.AttachTo(target.UIAnchor);
        }

        public void ClearHighlight()
        {
            if (_activeHighlight == null) return;
            Destroy(_activeHighlight.gameObject);
            _activeHighlight = null;
        }

        public void RenderArrow(ITutorialTarget target)
        {
            ClearArrow();
            if (target?.UIAnchor == null) return;
            _activeArrow = Instantiate(arrowPrefab, arrowLayer);
            _activeArrow.PointTo(target.UIAnchor);
        }

        public void ClearArrow()
        {
            if (_activeArrow == null) return;
            Destroy(_activeArrow.gameObject);
            _activeArrow = null;
        }

        public void ClearAll()
        {
            ClearInstruction();
            ClearHighlight();
            ClearArrow();
        }
    }
}
