using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.UI.Buildings;
using Galactic1.Game.UI.Buildings.DTO;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Универсальная панель здания.
    /// Сама по себе ничего не решает — только активирует модули.
    /// </summary>
    public class FacilityPanelView : MonoBehaviour
    {
        [SerializeField] private PremiumCurrencyWidget currencyWidget;
        [SerializeField] private List<FacilityPanelModule> modules;


        public CommonActionsPanelModule CommonActionsModule {get; private set;}
        public FacilityUpgradePanelModule UpgradeModule { get; private set; }


        
        public void Prewarm()
        {
            foreach (var module in modules)
            {
                module.gameObject.SetActive(true);

                switch (module)
                {
                    case CommonActionsPanelModule m:
                        CommonActionsModule = m;
                        break;
                    case FacilityUpgradePanelModule m:
                        UpgradeModule = m;
                        break;
                }
            }
        }

        public void Bind(FacilityDTO dto, object sceneAdapter, FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            foreach (var module in modules)
            {
                module.IsUpgradeable(dto); 
                module.BindIfSupported(dto, sceneAdapter, upgradeAdapter);
            }
            
            // отображаем валюту
            currencyWidget.gameObject.SetActive(true);
            
            gameObject.SetActive(true);
        }

        public void Rebind(FacilityDTO dto)
        {
            foreach (var module in modules)
                if (module.IsBound)
                {
                    module.IsUpgradeable(dto); 
                    module.Rebind(dto);
                }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            foreach (var module in modules)
                module.Unbind();
            
            ServiceLocator.Current.Get<GameSession>().SaveIfDirty();
        }
    }
}