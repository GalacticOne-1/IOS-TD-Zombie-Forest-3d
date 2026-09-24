using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Scene bridge — прямоугольная XZ-область для ограничения ручной камеры на
    /// конкретном tutorial-шаге. Тот же OnEnable/OnDisable регистрационный lifecycle, что
    /// TutorialTargetBehaviour/WorldTutorialTargetBehaviour уже используют для
    /// TutorialTargetRegistry — здесь только другой registry (TutorialCameraBoundsRegistry).
    /// Дизайнер ставит объект в сцену, растягивает Size в инспекторе/через Gizmos.</summary>
    public sealed class TutorialCameraBoundsBehaviour : MonoBehaviour, ITutorialCameraBoundsTarget
    {
        [Tooltip("Стабильный id (RuntimeId-ассет), на который ссылается " +
                 "TutorialCameraConstraintDefinition.boundsTargetId.")]
        [SerializeField] private TutorialTargetId targetId;

        [Tooltip("Размер прямоугольной области (X/Z используются для clamp, Y игнорируется — " +
                 "раздел 18 ТЗ: tutorial bounds ограничивают только XZ).")]
        [SerializeField] private Vector3 size = new(10f, 1f, 10f);

        public TutorialTargetId TargetId => targetId;
        public Bounds WorldBounds => new(transform.position, size);

        private void OnEnable()
        {
            if (targetId == null)
            {
                Debug.LogWarning($"[TutorialCameraBoundsBehaviour] '{name}' has empty targetId — skipped registration.");
                return;
            }
            ServiceLocator.Current.Get<TutorialCameraBoundsRegistry>().Register(this);
        }

        private void OnDisable()
        {
            if (targetId == null) return;
            ServiceLocator.Current.Get<TutorialCameraBoundsRegistry>().Unregister(this);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.15f);
            Gizmos.DrawCube(transform.position, size);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.9f);
            Gizmos.DrawWireCube(transform.position, size);
        }
#endif
    }
}