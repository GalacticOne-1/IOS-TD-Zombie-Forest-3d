
using Galactic1.Configs;
using Galactic1.Core.UI;
using Galactic1.Gameplay.Interaction.Objects;
using Galactic1.Gameplay.UI;
using Galactic1.Structs.UI;
using Galactic1.UI.Core;


namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Центральный менеджер отображения кнопок действия и атаки.
    /// Получает текущий интерактивный объект и включает нужную кнопку UI.
    /// Поддерживает:
    /// - ActionButton для сундуков, сейфов, трупов
    /// - AttackButton для врагов
    /// - Авто-выключение кнопок при недоступных объектах
    /// - Поддержка highlight
    /// </summary>
    public class ActionRules
    {
        private UIButtonAction actionButton;
        private UIButtonAttack attackButton;
        private TargetHPBarUI targetHPBar;
        
        private InteractionIcons iconBase;

        private IInteractable currentInteractable;


        
        public ActionRules(
            UIButtonAction actionButton, 
            UIButtonAttack attackButton, 
            TargetHPBarUI targetHpBar)
        {
            this.actionButton = actionButton;
            this.attackButton = attackButton;
            targetHPBar = targetHpBar;
            
            iconBase = ServiceLocator.Current.Get<ConfigProvider>().Get<UIStyleDatabase>().InteractionIcons;

            OnCurrentInteractableChanged(null);
            
            // Подписка на смену интерактивного объекта
            ServiceLocator.Current.Get<InteractionSystem>().OnCurrentChanged += OnCurrentInteractableChanged;
        }



        private void OnCurrentInteractableChanged(IInteractable interactable)
        {
            currentInteractable = interactable;

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            // Сначала скрываем все кнопки
            actionButton.Hide(iconBase.GetIconFor(null));
            attackButton.Hide();

            if (currentInteractable == null)
                return;

            //if (!currentInteractable.CanInteract(ServiceLocator.Current.Get<PlayerController>().transform))
                //return;

            // Определяем тип объекта и кнопку
            switch (currentInteractable)
            {
                case EnemyInteractable _:
                    attackButton.Show();
                    //targetHPBar
                    break;
                
                // +...
                case HomeContainerInteractable _:
                    actionButton.Show(iconBase.GetIconFor(currentInteractable));
                    break;

                default:
                    actionButton.Show(iconBase.GetIconFor(currentInteractable));
                    break;
            }
        }
    }
}
