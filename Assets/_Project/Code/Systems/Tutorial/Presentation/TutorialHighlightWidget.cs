using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Позиционирует себя поверх RectTransform таргета. Визуал (рамка/пульсация)
    /// настраивается в префабе — этот класс отвечает только за геометрию привязки.</summary>
    public sealed class TutorialHighlightWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform selfRect;

        public void AttachTo(RectTransform target)
        {
            selfRect.SetParent(target, worldPositionStays: false);
            selfRect.anchorMin = Vector2.zero;
            selfRect.anchorMax = Vector2.one;
            selfRect.offsetMin = Vector2.zero;
            selfRect.offsetMax = Vector2.zero;
        }
    }
}
