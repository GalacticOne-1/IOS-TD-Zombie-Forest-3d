using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    public class DragIcon : MonoBehaviour
    {
        [SerializeField] private Image icon;

        private RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            // Важно! Чтобы не блокировать OnDrop
            var cg = GetComponent<CanvasGroup>();
            if (cg == null)
                cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        }

        public void SetSprite(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        public void Follow(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPos
            );
            rectTransform.localPosition = localPos;
        }
    }
}