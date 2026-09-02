
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Связывает внешний Scrollbar со ScrollRect.
    /// Синхронизация двусторонняя:
    /// • скролл контента → двигает scrollbar
    /// • скролл scrollbar → двигает контент
    /// </summary>
    public sealed class ExternalScrollbarBinder : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Scrollbar  scrollbar;

        public Scrollbar Scrollbar => scrollbar;

        private bool syncing; // предотвращает рекурсивные вызовы

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
            scrollbar.onValueChanged.AddListener(OnScrollbarChanged);

            // начальная синхронизация
            scrollbar.value = scrollRect.verticalNormalizedPosition;
        }

        private void OnDestroy()
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
            scrollbar.onValueChanged.RemoveListener(OnScrollbarChanged);
        }

        // ScrollRect → Scrollbar
        private void OnScrollRectChanged(Vector2 pos)
        {
            if (syncing) return;
            syncing = true;
            scrollbar.value = pos.y;
            syncing = false;
        }

        // Scrollbar → ScrollRect
        private void OnScrollbarChanged(float value)
        {
            if (syncing) return;
            syncing = true;
            scrollRect.verticalNormalizedPosition = value;
            syncing = false;
        }
    }
}
