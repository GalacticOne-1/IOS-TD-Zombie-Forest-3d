using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.World
{
    public sealed class WorldToastItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private float riseSpeed = 1.5f;
        [SerializeField] private float duration = 1.2f;
        [SerializeField] private float fadeOutTime = 0.35f;

        private Transform _cameraTransform;
        private Coroutine routine;
        private Action<WorldToastItem> onComplete;


        public void Setup(Transform camera)
        {
            _cameraTransform = camera;
            gameObject.SetActive(false);
        }

        public void Play(
            Vector3 worldPosition,
            string text,
            Color color,
            Action<WorldToastItem> onComplete)
        {
            transform.position = worldPosition;
            label.text = text;
            label.color = color;
            this.onComplete = onComplete;
            
            // устанавливаем размер элемента 
            Vector2 size = label.GetPreferredValues(text);
            var rt = label.rectTransform;
            rt.sizeDelta = size;

            gameObject.SetActive(true);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            canvasGroup.alpha = 1f;
            var origin = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // поворачиваем лицом к камере
                //transform.rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);
                
                elapsed += UnityEngine.Time.deltaTime;

                transform.position = origin + Vector3.up * (riseSpeed * elapsed);

                float fadeStart = duration - fadeOutTime;
                if (elapsed > fadeStart)
                    canvasGroup.alpha = 1f - (elapsed - fadeStart) / fadeOutTime;

                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            routine = null;
            onComplete?.Invoke(this);
        }
    }
}