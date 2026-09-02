using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// Центральный роутер UI-инпута.
    /// Управляет фокусом и рассылает "click outside".
    /// </summary>
    public sealed class UIInputRouter
    {
        private readonly List<IUIFocusable> _focusables = new();

        private IUIFocusable _active;

        private readonly PointerEventData _pointerData;
        private readonly List<RaycastResult> _raycastResults = new();

        public UIInputRouter(EventSystem eventSystem)
        {
            _pointerData = new PointerEventData(eventSystem);

            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(() =>
            {
                _focusables.Clear();
            }));
        }

        // =========================
        // Register
        // =========================
        public void Register(IUIFocusable focusable)
        {
            if (!_focusables.Contains(focusable))
                _focusables.Add(focusable);
        }

        public void Unregister(IUIFocusable focusable)
        {
            _focusables.Remove(focusable);

            if (_active == focusable)
                _active = null;
        }


        // =========================
        // Focus control
        // =========================
        public void SetFocus(IUIFocusable focusable)
        {
            if (_active == focusable)
                return;

            _active?.OnFocusLost();
            _active = focusable;
        }

        public void ClearFocus()
        {
            _active?.OnFocusLost();
            _active = null;
        }

        // =========================
        // Input
        // =========================
        public void ProcessPointerDown(Vector2 screenPos)
        {
            // 1. если нет фокуса — ничего делать не нужно
            if (_active == null)
                return;

            // 2. если кликнули внутрь текущего — игнор
            if (_active.ContainsScreenPoint(screenPos))
                return;

            // 3. raycast UI → вдруг клик по другому фокусному элементу
            var next = RaycastForFocusable(screenPos);

            // 4. сравнение приоритетов
            if (next != null && next.Priority >= _active.Priority)
            {
                _active.OnFocusLost();
                _active = next;
                return;
            }

            // 5. иначе — просто потеря фокуса
            _active.OnFocusLost();
            _active = null;
        }

        // =========================
        // Helpers
        // =========================
        private IUIFocusable RaycastForFocusable(Vector2 screenPos)
        {
            _pointerData.position = screenPos;
            _raycastResults.Clear();

            EventSystem.current.RaycastAll(_pointerData, _raycastResults);

            for (int i = 0; i < _raycastResults.Count; i++)
            {
                var go = _raycastResults[i].gameObject;

                for (int j = 0; j < _focusables.Count; j++)
                {
                    if (_focusables[j] is Component c && c.gameObject == go)
                        return _focusables[j];
                }
            }

            return null;
        }
        
        
    }
}