using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.TimeWorld;
using Galactic1.Code.WorldMap;
using Galactic1.Systems;
using Galactic1.UI.Core;
using Galactic1.UI.Shop;
using TMPro;
using UnityEngine;

namespace Galactic1.Core.UI.HUD
{
    public class HUDMap : UIScreenPanel
    {
        [Header("World Time")] 
        [SerializeField] private SkipDayButton SkipDaysButton;
        [SerializeField] private TimeAlertView TimeAlertView;

        [SerializeField] private TMP_Text CurrentLocationText;

        [Header("Menu")] 
        [SerializeField] private GameObject settingsButton;
        [SerializeField] private GameObject gameShopButton;
        [SerializeField] private GameObject inventoryButton;
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);
            
            // =======
            SkipDaysButton.Activate();
            TimeAlertView.Activate(container.Resolve<CampDefensePreparationService>());

            // подписываем кнопку для перехода к текущей локации
            var mapController = ServiceLocator.Current.Get<WorldMapController>();
            mapController.OnLocationChanged += _ => CurrentLocationText.text = _.Config.Header.TitleLid;
            CurrentLocationText.gameObject.RegisterButtonClick(mapController.ToCurrentLocation);
            
            
            BindButtons(container);
        }

        public override void Remove()
        {
            base.Remove();
        }


        /// <summary>
        /// Для регистрации действия кнопок
        /// </summary>
        void BindButtons(DIContainer container)
        {
            settingsButton.RegisterButtonClick(container.Resolve<GameSettingsSystem>().ShowWindow);
            gameShopButton.RegisterButtonClick(container.Resolve<GameStoreService>().ShowWindow);
            
            // inventory
            inventoryButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                    _ =>
                    {
                        _.GetComponent<InventoryManagementWindow>().modeController.Open(InventoryGameplayMode.Transport_SquadOnly);
                    });
            });
        }

        
        
    }
}