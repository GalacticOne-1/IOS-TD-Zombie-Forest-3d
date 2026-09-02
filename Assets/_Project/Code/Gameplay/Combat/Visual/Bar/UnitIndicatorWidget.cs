
using Galactic1.PoolObject;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.HUD.Enemy
{
    /// <summary>
    /// Чисто презентационный виджет полоски здоровья врага.
    ///
    /// Не содержит игровой логики, подписок на события или реактивных свойств.
    /// Единственные обязанности: отобразить HP, позицию, видимость.
    ///
    /// Владелец всего жизненного цикла — EnemyHealthBarSystem.
    ///
    /// Жизненный цикл:
    ///   Pool.Get()        → OnSpawn()
    ///   system.Bind()     → TrackedTransform заполнен
    ///   system.SetHealth  → fillImage обновлён
    ///   system.Show/Hide  → canvasGroup.alpha изменён
    ///   Pool.Return()     → OnDespawn() → Unbind()
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UnitIndicatorWidget : PoolableMonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _healthFill;

        [SerializeField] private RectTransform _statusRoot;
        [SerializeField] private RectTransform _threatRoot;
        

        // ── Состояние ─────────────────────────────────────────────────

        private RectTransform _rect;
        
        /// <summary>
        /// Transform врага в мировом пространстве.
        /// Используется системой для проецирования позиции на экран.
        /// Null если виджет не привязан.
        /// </summary>
        public Transform TrackedTransform { get; private set; }
        
        public bool IsVisible => _canvasGroup.alpha > 0f;


        protected override void Awake()
        {
            base.Awake();
            _rect = (RectTransform)transform;
        }

        // ── Bind / Unbind ─────────────────────────────────────────────

        /// <summary>
        /// Привязать виджет к Transform врага.
        /// Вызывается EnemyHealthBarSystem сразу после Pool.Get().
        /// </summary>
        public void Bind(Transform worldRoot)
        {
            TrackedTransform = worldRoot;
        }

        /// <summary>
        /// Отвязать виджет. Вызывается автоматически в OnDespawn().
        /// </summary>
        public void Unbind()
        {
            TrackedTransform = null;
        }

        // ── Презентация ───────────────────────────────────────────────

        /// <summary>
        /// Обновить заполнение полоски HP.
        /// Вызывается системой при получении HealthChangedEvent.
        /// </summary>
        public void SetHealth(float currentHp, float maxHp)
        {
#if UNITY_EDITOR
            DLog.Alert($"Setting health to {currentHp}/{maxHp}", EDlogColor.YELLOW);
#endif

            if (maxHp <= 0f) return;
            _healthFill.fillAmount = Mathf.Clamp01(currentHp / maxHp);
        }

        /// <summary>
        /// Обновить экранную позицию виджета.
        /// Вызывается системой каждый LateUpdate когда враг в фрустуме.
        /// </summary>
        public void UpdateScreenPosition(Vector2 screenPos, Canvas canvas)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out var localPoint);

            _rect.anchoredPosition = localPoint;
        }

        /// <summary>Показать виджет. Вызывается системой при попадании и входе в фрустум.</summary>
        public void Show() => _canvasGroup.alpha = 1f;

        /// <summary>Скрыть виджет. Вызывается системой при выходе из фрустума или истечении таймера.</summary>
        public void Hide() => _canvasGroup.alpha = 0f;

        // ── PoolableMonoBehaviour ─────────────────────────────────────

        public override void OnSpawn()
        {
            base.OnSpawn();
            _rect.anchoredPosition = new Vector2(-9999f, -9999f);
            _healthFill.fillAmount = 1f;
            Hide();
        }

        public override void OnDespawn()
        {
            Unbind();
            base.OnDespawn();
        }
    }
}