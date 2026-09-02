using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// Circle AOE визуал (grenade)
    /// </summary>
    public sealed class CircleAbilityVisual : IAbilityVisual
    {
        private readonly GameObject _go;
        private readonly Material _mat;

        private static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
        private static readonly int RingColorId = Shader.PropertyToID("_RingColor");
        private static readonly int OuterColorId = Shader.PropertyToID("_OuterColor");

        private Color _validRing;
        private Color _validOuter;

        private static readonly Color InvalidRing = new(1f, 0.15f, 0.1f, 1f);
        private static readonly Color InvalidOuter = new(1f, 0.15f, 0.1f, 0.25f);

        private bool _lastValid = true;

        public CircleAbilityVisual(GameObject go)
        {
            _go = go;
            var renderer = go.GetComponentInChildren<Renderer>();
            _mat = renderer != null ? renderer.material : null;

            if (_mat == null) return;
            _validRing = _mat.GetColor(RingColorId);
            _validOuter = _mat.GetColor(OuterColorId);
        }

        // Вызывается один раз при StartTargeting
        public void SetRadius(float smallRadius, float bigRadius)
        {
            _mat?.SetFloat(InnerRadiusId, smallRadius);
            _mat?.SetFloat(OuterRadiusId, bigRadius);
        }

        public void Show() => _go.SetActive(true);
        public void Hide() => _go.SetActive(false);

        public void Update(Vector3 position, bool valid)
        {
            position.y = 0.2f;
            _go.transform.position = position;

            if (_mat == null || valid == _lastValid) return;
            _lastValid = valid;

            _mat.SetColor(RingColorId, valid ? _validRing : InvalidRing);
            _mat.SetColor(OuterColorId, valid ? _validOuter : InvalidOuter);
        }
    }
}