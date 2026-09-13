using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>World-аналог TutorialTargetBehaviour — для зданий/объектов сцены, а не
    /// UI-элементов. UIAnchor всегда null (не RectTransform), WorldAnchor = transform.
    /// Тот же регистрационный контракт (TutorialTargetRegistry), тот же OnEnable/OnDisable
    /// lifecycle. Отдельный компонент, а не расширение FacilityInstance — вешается как
    /// sibling на GameObject конкретного здания, не требует правок в FacilityInstance
    /// кроме одной строки в OnInteract() (см. её докстринг).</summary>
    public sealed class WorldTutorialTargetBehaviour : MonoBehaviour, ITutorialTarget
    {
        [Tooltip("Стабильный id (RuntimeId-ассет), на который ссылаются guidance/presentation " +
                 "*TargetId поля, и который используется как TargetId клика (см. FacilityInstance.OnInteract).")]
        [SerializeField] private TutorialTargetId targetId;

        public TutorialTargetId TargetId => targetId;
        public RectTransform UIAnchor => null;
        public Transform WorldAnchor => transform;

        private void OnEnable()
        {
            if (targetId == null)
            {
                Debug.LogWarning($"[WorldTutorialTargetBehaviour] '{name}' has empty targetId — skipped registration.");
                return;
            }
            ServiceLocator.Current.Get<TutorialTargetRegistry>().Register(this);
        }

        private void OnDisable()
        {
            if (targetId == null) return;
            ServiceLocator.Current.Get<TutorialTargetRegistry>().Unregister(this);
        }
    }
}