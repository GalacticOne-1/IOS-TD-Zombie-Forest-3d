using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>World-space аналог TutorialHighlightWidget — для не-UI таргетов
    /// (см. WorldTutorialTargetBehaviour). В отличие от UI-версии (SetParent внутрь
    /// RectTransform таргета), здесь виджет живёт в мировых координатах и сам следует
    /// за позицией таргета каждый кадр — по тому же паттерну, что TutorialArrowWidget.
    /// Визуал (кольцо/декаль/пульсация под зданием) настраивается в префабе — этот
    /// класс отвечает только за позиционирование.</summary>
    public sealed class TutorialWorldHighlightWidget : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = Vector3.zero;

        private Transform _target;

        public void AttachTo(Transform target)
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
            transform.position = _target.position + offset;
        }
    }
}