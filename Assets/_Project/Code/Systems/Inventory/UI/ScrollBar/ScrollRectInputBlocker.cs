
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Блокирует прямой drag-ввод по ScrollRect.
    /// Контент двигается только через внешний Scrollbar.
    /// 
    /// Вешается на тот же GameObject что и ScrollRect.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollRectInputBlocker : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IScrollHandler
    {
        private ScrollRect scrollRect;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        // Блокируем все drag-события — просто не пробрасываем в ScrollRect
        public void OnBeginDrag(PointerEventData eventData) { }
        public void OnDrag(PointerEventData eventData)      { }
        public void OnEndDrag(PointerEventData eventData)   { }

        // Блокируем скролл колёсиком мыши
        public void OnScroll(PointerEventData eventData)    { }
    }
}