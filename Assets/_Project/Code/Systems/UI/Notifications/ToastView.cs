
using System.Collections;
using UnityEngine;
using TMPro;

namespace Galactic1.UI.Notifications
{
    /// <summary>
    /// Визуальное представление тоста.
    /// </summary>
    public sealed class ToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;

        public TMP_Text MessageText => messageText;

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            gameObject.GetChild(0).SetActive(true);
            RectTransform = GetComponent<RectTransform>();
            canvasGroup.alpha = 0f;
        }

        public IEnumerator PlayIn(string message)
        {
            messageText.text = message;

            float t = 0f;
            const float duration = 0.2f;

            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = t / duration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        public IEnumerator PlayOut()
        {
            float t = 0f;
            const float duration = 0.2f;

            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - (t / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
    }
}