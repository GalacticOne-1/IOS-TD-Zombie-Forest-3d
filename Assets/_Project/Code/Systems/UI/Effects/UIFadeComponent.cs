namespace Galactic1.Code.UI.Common.Effects
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Универсальный компонент fade + width expand.
    /// Только визуальный слой.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIFadeComponent : MonoBehaviour
    {
        [Header("Fade")]
        [SerializeField] private float defaultDuration = 0.25f;
        [SerializeField] private bool disableRaycastWhenInvisible = true;

        [Header("Width Expand")]
        [SerializeField] private bool enableWidthExpand = true;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Coroutine _activeRoutine;
        private bool _outerCoroutine;

        public float Alpha => _canvasGroup.alpha;

        public void Setup()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        public void FadeIn(bool outerCoroutine, Action onComplete = null, float? duration = null)
        {
            Animate(1f, duration ?? defaultDuration, onComplete, outerCoroutine);
        }

        public void FadeOut(Action onComplete = null, float? duration = null)
        {
            Animate(0f, duration ?? defaultDuration, onComplete, false);
        }

        public void SetInstant(float alpha)
        {
            StopActiveRoutine();

            _canvasGroup.alpha = alpha;

            if (enableWidthExpand)
                SetScaleX(alpha);

            UpdateRaycastState();
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        private void Animate(float targetAlpha, float duration, Action onComplete, bool outerCoroutine)
        {
            _outerCoroutine = outerCoroutine;
            StopActiveRoutine();

            if (outerCoroutine)
                _activeRoutine = ServiceLocator.Current.Get<CoroutineController>()
                    .StartCoroutine(AnimateRoutine(targetAlpha, duration, onComplete));
            else
                _activeRoutine = StartCoroutine(AnimateRoutine(targetAlpha, duration, onComplete));
        }

        private IEnumerator AnimateRoutine(float targetAlpha, float duration, Action onComplete)
        {
            float startAlpha = _canvasGroup.alpha;
            float startScaleX = _rectTransform.localScale.x;

            float targetScaleX = enableWidthExpand ? targetAlpha : startScaleX;

            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);

                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalized);
                _canvasGroup.alpha = currentAlpha;

                if (enableWidthExpand)
                {
                    float currentScaleX = Mathf.Lerp(startScaleX, targetScaleX, normalized);
                    SetScaleX(currentScaleX);
                }

                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            if (enableWidthExpand)
                SetScaleX(targetScaleX);

            UpdateRaycastState();

            _activeRoutine = null;
            onComplete?.Invoke();
        }

        private void SetScaleX(float x)
        {
            Vector3 scale = _rectTransform.localScale;
            scale.x = Mathf.Max(0.0001f, x);
            _rectTransform.localScale = scale;
        }

        private void StopActiveRoutine()
        {
            if (_activeRoutine != null)
            {
                if (_outerCoroutine)
                    ServiceLocator.Current.Get<CoroutineController>().StopCoroutine(_activeRoutine);
                else
                    StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
        }

        private void UpdateRaycastState()
        {
            if (!disableRaycastWhenInvisible)
                return;

            bool visible = _canvasGroup.alpha > 0.001f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void OnDisable()
        {
            StopActiveRoutine();
        }
    }
}