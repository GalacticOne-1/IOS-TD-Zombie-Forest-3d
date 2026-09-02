using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Common.Effects
{
    public interface IUIFlashEffect
    {
        void Play(System.Action onComplete = null);
        void ResetFlash();
    }
    
    public sealed class UIFlashEffectShader : MonoBehaviour, IUIFlashEffect
    {
        [SerializeField] private Graphic _graphic; // Image / RawImage / TMP-compatible via material
        [SerializeField] private float _duration = 0.15f;

        private Material _runtimeMaterial;
        private Coroutine _routine;

        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

        private void EnsureRuntimeMaterial()
        {
            // если материал сменился снаружи — пересоздаём инстанс
            if (_runtimeMaterial == null || _graphic.material != _runtimeMaterial)
            {
                _runtimeMaterial = Instantiate(_graphic.material);
                _graphic.material = _runtimeMaterial;
            }
        }

        public void Play(System.Action onComplete = null)
        {
            EnsureRuntimeMaterial(); 
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(FlashRoutine(onComplete));
        }

        private IEnumerator FlashRoutine(System.Action onComplete)
        {
            float t = 0f;

            while (t < _duration)
            {
                t += Time.deltaTime;

                float v = 1f - (t / _duration);
                _runtimeMaterial.SetFloat(FlashAmount, v);

                yield return null;
            }

            _runtimeMaterial.SetFloat(FlashAmount, 0f);

            onComplete?.Invoke();
        }

        public void ResetFlash()
        {
            if (_runtimeMaterial != null)
                _runtimeMaterial.SetFloat(FlashAmount, 0f);
        }

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