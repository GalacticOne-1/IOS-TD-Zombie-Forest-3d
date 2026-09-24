using System.Collections.Generic;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Scene/UI renderer tutorial presentation.
    ///
    /// Может одновременно отображать несколько highlight-targets.
    /// Arrow остаётся single-target.
    ///
    /// Lifecycle presentation-объектов принадлежит этому renderer:
    /// TutorialPresentationService только резолвит targets и передаёт их сюда.
    /// </summary>
    public sealed class TutorialHUDController : UIScreenPanel, ITutorialPresentationRenderer
    {
        [SerializeField] private RectTransform highlightLayer;
        [SerializeField] private RectTransform arrowLayer;
        [SerializeField] private TutorialHighlightWidget highlightPrefab;
        [SerializeField] private TutorialArrowWidget arrowPrefab;
        [SerializeField] private TutorialWorldHighlightWidget worldHighlightPrefab;

        private readonly List<TutorialHighlightWidget> _activeHighlights = new();
        private readonly List<TutorialWorldHighlightWidget> _activeWorldHighlights = new();

        private TutorialArrowWidget _activeArrow;

        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            gameObject.SetActive(true);

            ServiceLocator.Current.Get<TutorialPresentationService>().AttachRenderer(this);
        }

        public override void Remove()
        {
            ServiceLocator.Current.Get<TutorialPresentationService>().DetachRenderer(this);

            ClearAll();
        }

        public void RenderHighlights(IReadOnlyList<ITutorialTarget> targets)
        {
            ClearHighlight();

            if (targets == null || targets.Count == 0)
                return;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                if (target.UIAnchor != null)
                {
                    var highlight = Instantiate(
                        highlightPrefab,
                        highlightLayer);

                    highlight.AttachTo(target.UIAnchor);
                    _activeHighlights.Add(highlight);
                }
                else if (target.WorldAnchor != null)
                {
                    // World-таргеты (здания и т.п.) не имеют RectTransform —
                    // отдельный виджет позиционируется в мировых координатах.
                    var worldHighlight = Instantiate(worldHighlightPrefab);

                    worldHighlight.AttachTo(target.WorldAnchor);
                    _activeWorldHighlights.Add(worldHighlight);
                }
            }
        }

        public void ClearHighlight()
        {
            for (int i = 0; i < _activeHighlights.Count; i++)
            {
                var highlight = _activeHighlights[i];

                if (highlight != null)
                    Destroy(highlight.gameObject);
            }

            _activeHighlights.Clear();

            for (int i = 0; i < _activeWorldHighlights.Count; i++)
            {
                var worldHighlight = _activeWorldHighlights[i];

                if (worldHighlight != null)
                    Destroy(worldHighlight.gameObject);
            }

            _activeWorldHighlights.Clear();
        }

        /// <summary>Создаёт arrow и передаёт ему таргет. Тип таргета (UI/World) определяет
        /// сам TutorialArrowWidget.</summary>
        public void RenderArrow(ITutorialTarget target)
        {
            ClearArrow();

            if (target == null)
                return;

            _activeArrow = Instantiate(
                arrowPrefab,
                arrowLayer);

            _activeArrow.PointTo(target);
        }

        public void ClearArrow()
        {
            if (_activeArrow == null)
                return;

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