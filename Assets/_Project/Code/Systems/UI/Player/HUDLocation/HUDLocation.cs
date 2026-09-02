
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.Raid.Test;
using Galactic1.Code.UI.Inventory;
using Galactic1.Systems;
using Galactic1.UI.Core;
using Galactic1.UI.Shop;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Core.UI.HUD
{
    public class HUDLocation : UIScreenPanel
    {
        [Header("Menu")] 
        [SerializeField] private GameObject settingsButton;
        [SerializeField] private GameObject worldMapButton;
        [SerializeField] private GameObject gameShopButton;
        [SerializeField] private GameObject inventoryButton;
        
        
        [Space]
        [SerializeField] private GameObject cancelButton;
        [SerializeField] private GameObject defeatButton;
        [SerializeField] private GameObject completeButton;
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);
            
            // =======
            
            // регистрация действия кнопок
            settingsButton.RegisterButtonClick(container.Resolve<GameSettingsSystem>().ShowWindow);
            gameShopButton.RegisterButtonClick(container.Resolve<GameStoreService>().ShowWindow);
            // worldMapButton.RegisterButtonClick(() =>
            //     EventBus<WorldMapSceneRequestEvent>.Raise(new WorldMapSceneRequestEvent()));
            
            //worldMapButton.SetActive(!SystemRepository.CampDefense);
            worldMapButton.RegisterButtonClick(LocationTacticalDEBUG.RaidEvacuation);
            
            // inventory
            inventoryButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                    _ =>
                    {
                        _.GetComponent<InventoryManagementWindow>().modeController.Open(InventoryGameplayMode.Transport_SquadOnly);
                    });
            });
            
            
            
            // test
            cancelButton.RegisterButtonClick(LocationTacticalDEBUG.RaidCancel);
            defeatButton.RegisterButtonClick(LocationTacticalDEBUG.RaidDefeat);
            completeButton.RegisterButtonClick(LocationTacticalDEBUG.RaidComplete);
        }

        public override void Remove()
        {
            base.Remove();
        }


        
        
    }
}