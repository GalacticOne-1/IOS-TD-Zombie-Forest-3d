using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Указывает на RectTransform таргета. Простейшая реализация —
    /// позиционирование рядом с таргетом.</summary>
    public sealed class TutorialArrowWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform selfRect;
        [SerializeField] private Vector2 offsetFromTarget = new(0, 60f);

        private RectTransform _target;

        public void PointTo(RectTransform target)
        {
            _target = target;
            UpdatePosition();
        }

        private void LateUpdate()
        {
            if (_target != null)
                UpdatePosition();
        }

        private void UpdatePosition()
        {
            selfRect.position = _target.position + (Vector3)offsetFromTarget;
        }
    }
}
