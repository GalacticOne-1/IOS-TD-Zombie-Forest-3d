using Galactic1.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Gameplay.UI
{
    /// <summary>
    /// Полоска здоровья текущей цели (как в LDoE).
    /// Подписывается на ITargetable и автоматически скрывается при смене или смерти цели.
    /// </summary>
    public class TargetHPBarUI : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private CanvasGroup canvasGroup;

        private ITargetable currentTarget;

        
        
        
        
        private void OnEnable()
        {
            ServiceLocator.Current.Get<InteractionSystem>().OnCurrentChanged += OnCurrentChanged;
        }

        private void OnDisable()
        {
            ServiceLocator.Current.Get<InteractionSystem>().OnCurrentChanged -= OnCurrentChanged;
            Unbind();
        }

        private void OnCurrentChanged(IInteractable current)
        {
            // Если цель та же самая — ничего не делаем
            if (currentTarget != null && current == currentTarget)
                return;

            Unbind();

            if (current is ITargetable targetable && targetable.IsAlive)
            {
                currentTarget = targetable;

                currentTarget.OnHealthChanged += OnHealthChanged;
                currentTarget.OnDied += OnDied;

                UpdateFill(currentTarget.Health, currentTarget.MaxHealth);

                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Unbind()
        {
            if (currentTarget != null)
            {
                currentTarget.OnHealthChanged -= OnHealthChanged;
                currentTarget.OnDied -= OnDied;
                currentTarget = null;
            }
        }

        private void OnHealthChanged(float hp)
        {
            if (currentTarget != null)
                UpdateFill(currentTarget.Health, currentTarget.MaxHealth);
        }

        private void OnDied()
        {
            Hide();

            // сбрасываем текущий интеракт
            ServiceLocator.Current.Get<InteractionSystem>()
                .SetCurrentInteractable(null);
        }

        private void UpdateFill(float hp, float max)
        {
            fill.fillAmount = Mathf.Clamp01(hp / max);
        }

        private void Show()
        {
            canvasGroup.alpha = 1;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0;
        }
    }
}
