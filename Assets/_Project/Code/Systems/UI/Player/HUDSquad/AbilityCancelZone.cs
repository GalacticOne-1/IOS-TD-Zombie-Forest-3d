using System.Collections.Generic;
using Galactic1.Code.Gameplay.Targeting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Core.UI.HUD
{
    public sealed class AbilityCancelZone : MonoBehaviour, ITargetingCancelZone
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private float normalAlpha = 0.5f;
        [SerializeField] private float highlightedAlpha = 0.8f;

        private readonly List<RaycastResult> _raycastResults = new();

        private bool _isHighlighted;

        public bool ContainsScreenPoint(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;

            if (eventSystem == null)
                return false;

            var pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition
            };

            _raycastResults.Clear();

            eventSystem.RaycastAll(pointerData, _raycastResults);

            foreach (var result in _raycastResults)
            {
                if (result.gameObject == gameObject)
                    return true;
            }

            return false;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (_isHighlighted == highlighted)
                return;

            _isHighlighted = highlighted;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = highlighted
                    ? highlightedAlpha
                    : normalAlpha;
            }
        }

        public void ResetHighlight()
        {
            _isHighlighted = false;

            if (canvasGroup != null)
                canvasGroup.alpha = normalAlpha;
        }

        private void OnDisable()
        {
            ResetHighlight();
            _raycastResults.Clear();
        }
    }
}