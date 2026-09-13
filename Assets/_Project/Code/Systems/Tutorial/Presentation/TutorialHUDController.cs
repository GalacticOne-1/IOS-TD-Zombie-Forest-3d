using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Scene-local рендерер Tutorial-специфичной презентации: ТОЛЬКО highlight/arrow/
    /// camera focus. Инструкция/прогресс/награда шага больше не здесь — это generic
    /// Scenario Task слой (см. Galactic1.Code.Gameplay.Tasks.Presentation.ScenarioTaskPanel),
    /// который Tutorial лишь ПОПОЛНЯЕТ через TutorialTaskPresenter, но не владеет и не
    /// рендерит напрямую.
    ///
    /// Обычный UIScreenPanel под _layerRoot.hudRoot, пересоздаётся при каждой загрузке
    /// сцены — presentation discovers active step, а не наоборот.
    ///
    /// НЕ IGameService — единственная точка обнаружения этого класса это
    /// AttachRenderer/DetachRenderer через TutorialPresentationService.
    ///
    /// ⚠️ Требует UIScreenId.TutorialHUD в enum + добавления в UIScreenManager.GetRoot
    /// и PreloadScreens.
    /// </summary>
    public sealed class TutorialHUDController : UIScreenPanel, ITutorialPresentationRenderer
    {
        [SerializeField] private RectTransform highlightLayer;
        [SerializeField] private RectTransform arrowLayer;
        [SerializeField] private TutorialHighlightWidget highlightPrefab;
        [SerializeField] private TutorialArrowWidget arrowPrefab;
        [SerializeField] private TutorialWorldHighlightWidget worldHighlightPrefab;

        private TutorialHighlightWidget _activeHighlight;
        private TutorialArrowWidget _activeArrow;
        private TutorialWorldHighlightWidget _activeWorldHighlight;

        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Get<TutorialPresentationService>().AttachRenderer(this);
        }

        public override void Remove()
        {
            ServiceLocator.Current.Get<TutorialPresentationService>().DetachRenderer(this);
        }

        public void RenderHighlight(ITutorialTarget target)
        {
            ClearHighlight();
            if (target == null) return;

            if (target.UIAnchor != null)
            {
                _activeHighlight = Instantiate(highlightPrefab, highlightLayer);
                _activeHighlight.AttachTo(target.UIAnchor);
            }
            else if (target.WorldAnchor != null)
            {
                // World-таргеты (здания и т.п.) не имеют RectTransform — отдельный виджет,
                // позиционируется в мировых координатах, не внутри Canvas-иерархии.
                _activeWorldHighlight = Instantiate(worldHighlightPrefab);
                _activeWorldHighlight.AttachTo(target.WorldAnchor);
            }
        }

        public void ClearHighlight()
        {
            if (_activeHighlight != null)
            {
                Destroy(_activeHighlight.gameObject);
                _activeHighlight = null;
            }

            if (_activeWorldHighlight != null)
            {
                Destroy(_activeWorldHighlight.gameObject);
                _activeWorldHighlight = null;
            }
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
            ClearHighlight();
            ClearArrow();
        }
    }
}