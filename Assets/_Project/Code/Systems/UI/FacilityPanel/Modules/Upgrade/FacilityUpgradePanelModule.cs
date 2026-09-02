
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Buildings;
using Galactic1.Game.UI.Buildings.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings
{
    /// <summary>
    /// Модуль панели здания отвечающий за upgrade.
    /// </summary>
    public sealed class FacilityUpgradePanelModule : FacilityPanelModule
    {
        [SerializeField] private GameObject closeButton;
        [SerializeField] private FacilityUpgradeView view;

        private FacilityUpgradeSceneAdapter _adapter;

        
        
        
        public override bool IsAutoActivate => false;
        public override bool IsSupported(FacilityDTO dto) => true;
        
        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter, 
            FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);

            _adapter = upgradeAdapter;

            closeButton.RegisterButtonClick(() => gameObject.SetActive(false));
            view.OnUpgradeClicked += HandleUpgrade;

            Rebind(dto);
        }

        public override void Rebind(FacilityDTO dto)
        {
            var upgrade = _adapter.GetUpgradeDetails();
            
            if (upgrade != null)
                view.Show(upgrade);
        }

        public override void Unbind()
        {
            base.Unbind();

            view.OnUpgradeClicked -= HandleUpgrade;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        

        private void HandleUpgrade()
        {
            if (_adapter.TryUpgrade())
            {
                Hide();
            }
        }
    }
}