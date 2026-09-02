using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Common.Effects
{
    // Вариант 2 — через overlay Image
    public sealed class UIFlashEffectOverlay : MonoBehaviour, IUIFlashEffect
    {
        [SerializeField] private Graphic _overlay;
        [SerializeField] private float _duration = 0.3f;

        private Coroutine _routine;


        private void Awake()
        {
            SetAlpha(0f);
        }

        public void Play(System.Action onComplete = null)
        {
            if (_routine != null) 
                StopCoroutine(_routine);
            
            _routine = StartCoroutine(FlashRoutine(onComplete));
        }

        private IEnumerator FlashRoutine(Action onComplete)
        {
            float half = _duration * 0.5f;
            float t = 0f;
            while (t < half)
            {
                t += Mathf.Min(Time.deltaTime, 1f / 30f);
                SetAlpha(Mathf.Clamp01(t / half));
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Mathf.Min(Time.deltaTime, 1f / 30f);
                SetAlpha(Mathf.Clamp01(1f - t / half));
                yield return null;
            }

            SetAlpha(0f);
            onComplete?.Invoke();
        }

        private void SetAlpha(float a)
        {
            var c = _overlay.color;
            c.a = a;
            _overlay.color = c;
        }

        public void ResetFlash() => SetAlpha(0f);

        private void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            ResetFlash();
        }
    }
}