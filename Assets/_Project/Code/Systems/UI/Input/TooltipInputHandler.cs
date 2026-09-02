using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Code.UI.Tooltips
{
    /// <summary>
    /// Студийный Input Handler для показа подсказок.
    /// Можно вешать на любой UI-элемент.
    /// Поддерживает click и long-press.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TooltipInputHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {
        [Header("Hold BasicSettings")]
        [SerializeField] private float holdThreshold = 0; // секунды

        private bool isPointerDown;
        private float pointerDownTime;

        /// <summary>
        /// Вызывается, когда нужно показать подсказку.
        /// Параметр: RectTransform для anchor.
        /// </summary>
        private Action<RectTransform> OnTooltipRequested;

        /// <summary>
        /// Вызывается при отмене (pointer up / exit)
        /// </summary>
        private Action OnTooltipCancelled;

        public void RegisterOnRequest(Action<RectTransform> e) => OnTooltipRequested = e;
        public void RegisterOnCancell(Action e) => OnTooltipCancelled = e;
        
        
        
        
        private void Update()
        {
            if (isPointerDown)
            {
                if (Time.unscaledTime - pointerDownTime >= holdThreshold)
                {
                    isPointerDown = false; // показываем один раз
                    OnTooltipRequested?.Invoke(transform as RectTransform);
                }
            }
        }

        #region IPointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            pointerDownTime = UnityEngine.Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelTooltip();
        }

        #endregion

        private void CancelTooltip()
        {
            isPointerDown = false;
            OnTooltipCancelled?.Invoke();
        }
    }
}
