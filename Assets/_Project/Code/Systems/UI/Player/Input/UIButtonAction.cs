
using UnityEngine;
using UnityEngine.UI;
using Galactic1.Gameplay.Interaction;
using Galactic1.UI.Core;

namespace Galactic1.Core.UI
{
    /// <summary>
    /// UI кнопка действия игрока
    /// - отображает иконку текущего взаимодействия
    /// - реагирует на нажатие
    /// </summary>
    public class UIButtonAction : BaseUIButton
    {
        [SerializeField] private Image iconImg;
        [SerializeField] private GameObject highlight;
        
        
        
        
        
        
        private void Start()
        {
            events.onClick.AddListener(() =>
            {
                // Сигнал системе взаимодействия выполнить действие
                ServiceLocator.Current.Get<InteractionSystem>().InteractCurrent(null); // можно передать transform игрока
            });
            
            events.onUp.AddListener(() =>
            {
                ServiceLocator.Current.Get<InteractionSystem>().ButtonUp();
            });
        }
        
       
        
        public void Hide(Sprite icon)
        {
            highlight.SetActive(false);
            iconImg.sprite = icon;
        }
        
        public void Show(Sprite icon)
        {
            highlight.SetActive(true);
            iconImg.sprite = icon;
        }
    }
}