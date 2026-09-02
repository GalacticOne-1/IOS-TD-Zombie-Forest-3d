
using UnityEngine;

namespace Galactic1.RaidLoot.Scene
{
    /// <summary>
    /// Управляет outline и emissive интенсивностью контейнера.
    ///
    /// Состояния:
    ///   Idle     → outline выключен
    ///   Detected → слабый outline (контейнер виден, игрок не в радиусе)
    ///   InRange  → сильный outline (игрок в радиусе, таймер идёт)
    ///
    /// Не хранит состояние контейнера — только реагирует на вызовы от View.
    /// Поддерживает плавный lerp между интенсивностями.
    /// </summary>
    public sealed class LootContainerHighlightView : MonoBehaviour
    {
        [Header("Renderers")] 
        [SerializeField] private Renderer[] _renderers;

        [Header("Outline intensities")] 
        [SerializeField] private float _detectedIntensity = 0.3f;
        [SerializeField] private float _inRangeIntensity = 1.0f;
        [SerializeField] private float _openingPulseAmplitude = 0.25f;
        [SerializeField] private float _openingPulseSpeed = 4f;
        [SerializeField] private float _lerpSpeed = 6f;

        [Header("Shader property")]
        [SerializeField] private string _emissiveIntensityProperty = "_EmissiveIntensity";

        private int _emissivePropId;
        private float _targetIntensity;
        private float _currentIntensity;
        private bool _isOpening;

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _emissivePropId = Shader.PropertyToID(_emissiveIntensityProperty);

            _renderers = gameObject.GetComponentsInChildren<Renderer>();

            // Читаем стартовые значения из материала (Inspector-настройки)
            if (_renderers != null && _renderers.Length > 0 && _renderers[0] != null)
            {
                var mat = _renderers[0].sharedMaterial;
                if (mat != null)
                {
                    _currentIntensity = _detectedIntensity;
                    _targetIntensity  = _currentIntensity;
                }
            }

            // Применяем сразу, чтобы MPB не обнулял материал на первом кадре
            ApplyToRenderers(_currentIntensity);
            //SetDetected(true);
        }

        private void Update()
        {
            float target = _targetIntensity;

            if (_isOpening)
            {
                target +=
                    Mathf.Sin(Time.time * _openingPulseSpeed)
                    * _openingPulseAmplitude;
            }

            _currentIntensity = Mathf.Lerp(
                _currentIntensity,
                target,
                Time.deltaTime * _lerpSpeed);

            ApplyToRenderers(_currentIntensity);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Контейнер виден игроку, но он вне радиуса.
        /// Включает мягкий outline.
        /// </summary>
        public void SetDetected(bool detected)
        {
            // Не снижаем если уже в InRange
            if (!detected && _targetIntensity > _detectedIntensity) return;

            _targetIntensity = detected ? _detectedIntensity : 0f;
        }

        public void SetDetected()
        {
            _isOpening = false;
            _targetIntensity = _detectedIntensity;
        }

        public void SetInRange()
        {
            _isOpening = false;
            _targetIntensity = _inRangeIntensity;
        }

        public void SetOpening()
        {
            _isOpening = true;
            _targetIntensity = _inRangeIntensity;
        }

        public void SetIdle()
        {
            _isOpening = false;
            _targetIntensity = 0f;
        }

        // ── Internal ─────────────────────────────────────────────────────────

        private void ApplyToRenderers(float intensity)
        {
            foreach (var r in _renderers)
            {
                if (r == null)
                    continue;

                r.GetPropertyBlock(_mpb);

                _mpb.SetFloat(
                    _emissivePropId,
                    intensity);

                r.SetPropertyBlock(_mpb);
            }
        }
        
        
        
        
        
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}