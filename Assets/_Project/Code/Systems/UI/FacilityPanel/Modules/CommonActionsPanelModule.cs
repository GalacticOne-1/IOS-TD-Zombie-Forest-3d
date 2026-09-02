
using System;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.BuildingPanel;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Общие действия для всех зданий:
    /// починка, удаление, выключение.
    /// </summary>
    public class CommonActionsPanelModule : FacilityPanelModule
    {
        [SerializeField] private GameObject demolishButton;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private GameObject upgradeButton;


        public event Action OnUpgradeRequested;
        
        
        
        public override bool IsSupported(FacilityDTO dto) => true;

        public override void IsUpgradeable(FacilityDTO dto)
        {
            upgradeButton.SetActive(dto.CanUpgrade);
        }

        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null, 
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);
            nameText.text = dto.DisplayName;
            var building = ServiceLocator.Current.Get<BaseFacilityRepository>().TryGet(dto.Id);
            
            // === close
            demolishButton.RegisterButtonClick(() =>
            {
                var data = new ConfirmPopupData(
                    "Confirm Demolish",
                    "Are you sure you want to delete facility?",
                    "Confirm",
                    onOk: () =>
                    {
                        //building.instance.Slot.ClearSlot();                                    
                        ServiceLocator.Current.Get<FacilityPanelController>().OnHide();
                    },
                    onClose: () => {  }
                );

                ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPopup, data);
                
            });
            
            // === upgrade
            upgradeButton.RegisterButtonClick(() =>
            {
                OnUpgradeRequested?.Invoke();
            });
        }

        public override void Rebind(FacilityDTO dto) {}

        public override void Unbind()
        {
            base.Unbind();
        }
    }
}