using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    /// <summary>
    /// Чисто визуальный компонент. Никакой игровой логики.
    ///
    /// ЗАГЛУШКА: используемый сейчас шейдер (URP/Lit) не имеет
    /// property "_Damage", поэтому повреждение имитируется через
    /// тонирование _BaseColor (Color.Lerp: здоровый -> повреждённый).
    ///
    /// Когда появится кастомный шейдер с реальным _Damage float —
    /// заменить ApplyDamage на SetFloat(_Damage, damage) и убрать
    /// _healthyColor/_damagedColor.
    /// </summary>
    public sealed class FacilityDamageVisualizer : MonoBehaviour
    {
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Color _healthyColor = Color.white;
        [SerializeField] private Color _damagedColor = Color.yellow;

        private MaterialPropertyBlock _propertyBlock;
        private Color[] _originalColors;
        private bool _initialized;

        private void EnsureReady()
        {
            if (_initialized)
                return;

            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<Renderer>(true);

            _propertyBlock = new MaterialPropertyBlock();
            _originalColors = new Color[_renderers.Length];

            for (var i = 0; i < _renderers.Length; i++)
            {
                var rend = _renderers[i];
                if (rend == null || rend.sharedMaterial == null)
                {
                    _originalColors[i] = Color.white;
                    continue;
                }

                _originalColors[i] = rend.sharedMaterial.HasProperty(BaseColorPropertyId)
                    ? rend.sharedMaterial.GetColor(BaseColorPropertyId)
                    : Color.white;
            }

            _initialized = true;
        }

        /// <summary>
        /// Вызывается сразу при Bind() — до первого попадания.
        /// </summary>
        public void Initialize(float currentHP, float maxHP)
        {
            EnsureReady();
            ApplyDamage(currentHP, maxHP);
        }

        /// <summary>
        /// Вызывается на каждое OnHealthChanged.
        /// </summary>
        public void SetHealth(float currentHP, float maxHP)
        {
            EnsureReady();
            ApplyDamage(currentHP, maxHP);
        }

        private void ApplyDamage(float currentHP, float maxHP)
        {
            float hp01 = maxHP > 0
                ? currentHP / maxHP
                : 0f;

            hp01 = Mathf.Clamp01(hp01);

            Color color = Color.Lerp(
                _damagedColor,
                _healthyColor,
                hp01);

            foreach (var renderer in _renderers)
            {
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(_propertyBlock);

                _propertyBlock.SetColor(
                    BaseColorPropertyId,
                    color);

                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}