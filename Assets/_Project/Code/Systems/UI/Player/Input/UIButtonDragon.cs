using Galactic1.Configs;
using Galactic1.Gameplay.Control;
using Galactic1.Gameplay.Interaction;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace Galactic1.Core.UI
{
    public class UIButtonDragon : BaseUIButton
    {
        [SerializeField] private Image iconImg;
        [SerializeField] private GameObject highlight;

        private CDragonUI dragonUI;
        
        
        
        private void Start()
        {
            // button icon
            dragonUI = ServiceLocator.Current.Get<ConfigProvider>().Get<UIStyleDatabase>().DragonUI;
            
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(_ =>
            {
                // var isDragon = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0].OnDragon.Value;
                // iconImg.sprite = isDragon ? dragonUI.footIcon : dragonUI.dragonIcon;
                // SetButton(false);
            }));
            
            
            // subscription
            events.onClick.AddListener(SwitchController);

            var dragonInteractionSystem = ServiceLocator.Current.Get<DragonInteractionSystem>();
            dragonInteractionSystem.OnDetectDragon.Subscribe(_ => SetButton(_));
            dragonInteractionSystem.OnDetectGround.Subscribe(_ => SetButton(_));
        }


        void SetButton(bool y)
        {
            highlight.SetActive(y);
        }
        
        // для смены управления объектом player/dragon
        void SwitchController()
        {
            // слезаем с дракона
            // if (ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0].OnDragon.Value)
            // {
            //     if (highlight.activeSelf)
            //         iconImg.sprite = dragonUI.dragonIcon;
            //     ControllableSwitcher.Button_OutDragon();
            // }
            //
            // // садимся на дракона
            // else
            // {
            //     if (highlight.activeSelf)
            //         iconImg.sprite = dragonUI.footIcon;
            //     ControllableSwitcher.Button_OnDragon();
            // }
        }
        
    }
}