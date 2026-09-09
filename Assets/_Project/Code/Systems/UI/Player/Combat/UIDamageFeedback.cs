using System.Collections;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Core.UI
{
    public sealed class UIDamageFeedback : UIScreenPanel, IGameService
    {
        [SerializeField] private CanvasGroup damageFrame;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine hideCoroutine;
        
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            ServiceLocator.Current.Register(this);
            
            gameObject.SetActive(true);
            damageFrame.alpha = 0f;
        }
        
        public override void Remove()
        {
            base.Remove();

            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(
                () => ServiceLocator.Current.Unregister<UIDamageFeedback>()));
        }

        public override void OnShow(object data = null)
        {
            damageFrame.alpha = 1f;

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine = StartCoroutine(HideRoutine());
        }

        private IEnumerator HideRoutine()
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / fadeDuration);
                damageFrame.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            damageFrame.alpha = 0f;
            hideCoroutine = null;
        }
    }
}