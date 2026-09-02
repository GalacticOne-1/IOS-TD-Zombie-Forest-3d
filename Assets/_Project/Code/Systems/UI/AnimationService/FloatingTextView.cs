using System.Collections;
using TMPro;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class FloatingTextView : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float fadeInTime = 0.25f;
        [SerializeField] private float fadeOutTime = 0.35f;
        [SerializeField] private float moveSpeed = 20f;

        [Header("Refs")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup group;

        public void Play(string text, Color color, Vector2 screenPos, float lifetime = 0f)
        {
            label.text = text;
            label.color = color;

            RectTransform rt = (RectTransform)transform;
            rt.position = screenPos;

            // Если lifetime > 0, пересчитаем длительности фаз
            if (lifetime > 0f)
            {
                float totalCurve = fadeInTime + fadeOutTime;

                fadeInTime  = lifetime * (fadeInTime / totalCurve);
                fadeOutTime = lifetime * (fadeOutTime / totalCurve);
            }

            StartCoroutine(AnimationRoutine());
        }

        private IEnumerator AnimationRoutine()
        {
            float t;

            // ------------------------------------------------------------
            // IN — Fade in + Move up
            // ------------------------------------------------------------
            t = 0f;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;

                float k = UIAnimationService.EaseOutQuad(t / fadeInTime);

                group.alpha = k;
                transform.localPosition += Vector3.up * (moveSpeed * Time.unscaledDeltaTime);

                yield return null;
            }

            // ------------------------------------------------------------
            // OUT — Fade out
            // ------------------------------------------------------------
            t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;

                float k = 1f - UIAnimationService.EaseInQuad(t / fadeOutTime);
                group.alpha = k;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
