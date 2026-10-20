
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Background
{
    /// <summary>
    /// Aspect Fill для фонового UI Image.
    /// Сохраняет пропорции спрайта и полностью заполняет viewport.
    /// Лишние области выходят за границы экрана.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class BackgroundAspectFill : MonoBehaviour
    {
        [SerializeField]
        private RectTransform viewport;

        private RectTransform _rectTransform;
        private Image _image;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _image = GetComponent<Image>();

            // Центрированное позиционирование изображения.
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled)
                Apply();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && isActiveAndEnabled)
                Apply();
        }

        private void Apply()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_image == null)
                _image = GetComponent<Image>();

            if (viewport == null || _image == null)
                return;

            Sprite sprite = _image.sprite;

            if (sprite == null)
                return;

            float viewWidth = viewport.rect.width;
            float viewHeight = viewport.rect.height;

            if (viewWidth <= 0f || viewHeight <= 0f)
                return;

            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;

            if (spriteWidth <= 0f || spriteHeight <= 0f)
                return;

            float aspect = spriteWidth / spriteHeight;

            // Aspect Fill: изображение должно покрыть обе оси.
            float width = Mathf.Max(
                viewWidth,
                viewHeight * aspect
            );

            float height = Mathf.Max(
                viewHeight,
                viewWidth / aspect
            );

            _rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
