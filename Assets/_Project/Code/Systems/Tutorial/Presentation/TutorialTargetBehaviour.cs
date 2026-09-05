using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Вешается на любой UI-элемент, который тутор должен уметь подсветить/
    /// указать стрелкой. Регистрация/снятие строго по OnEnable/OnDisable — никаких
    /// висящих подписок после ухода со сцены.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TutorialTargetBehaviour : MonoBehaviour, ITutorialTarget
    {
        [Tooltip("Стабильный id (RuntimeId-ассет), на который ссылаются TutorialPresentationDefinition.*TargetId.")]
        [SerializeField] private TutorialTargetId targetId;

        public TutorialTargetId TargetId => targetId;
        public RectTransform UIAnchor => transform as RectTransform;
        public Transform WorldAnchor => transform;

        private void OnEnable()
        {
            if (targetId == null)
            {
                Debug.LogWarning($"[TutorialTargetBehaviour] '{name}' has empty targetId — skipped registration.");
                return;
            }
            ServiceLocator.Current.Get<TutorialTargetRegistry>().Register(this);
        }

        private void OnDisable()
        {
            if (targetId == null) return;
            // Fix: Unregister теперь принимает инстанс, а не string id — см. TutorialTargetRegistry.
            ServiceLocator.Current.Get<TutorialTargetRegistry>().Unregister(this);
        }
    }
}
