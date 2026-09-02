
using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Runtime.Recruitment;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// UI-модуль панели таверны (найм юнитов).
    /// </summary>
    public class TavernPanelModule : FacilityPanelModule
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject noOffersPrefab, tavernOfferPrefab;
        [SerializeField] private GameObject refreshButton;
        [SerializeField] private TMP_Text autoRefreshText;
        [SerializeField] private TMP_Text capacityText;

        private List<TavernOfferCard> offerViews = new();

        private RecruitmentTavernSceneAdapter _adapter;
        
        
        
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.Tavern;

        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null, 
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);
            _adapter = sceneAdapter as RecruitmentTavernSceneAdapter;
            
            scrollRect.content.MakeEmptyImmediate();
            Rebind(dto);
        }

        public override void Unbind()
        {
            base.Unbind();
            scrollRect.content.MakeEmpty();
            offerViews.Clear();
        }

        public override void Rebind(FacilityDTO dto)
        {
            scrollRect.content.MakeHidden();
            offerViews.Clear();
            
            var details = dto.Details as TavernDetailsDTO;
            if (details == null)
                return;
            
            
            capacityText.text = $"{details.UsedSlots}/{details.CampCapacity}";
            
            // === refresh
            refreshButton.GetChild(0).CMP_Text().text = _adapter.GetPremiumRefreshCost().ToString();
            refreshButton.ButtonSetInteractable(_adapter.CanPremiumRefresh());
            refreshButton.RegisterButtonClick(OnPremiumRefreshClicked);
            
            autoRefreshText.text = $"*Auto refresh in {_adapter.DaysUntilRefresh()} days";
            
            // === offers list
            var uiRootView = ServiceLocator.Current.Get<UIRootView>();
            var styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();
            var portraitCache = ServiceLocator.Current.Get<CharacterPortraitCache>();

            var l = details.Offers.Count;
            if (l > 0)
            {
                for (int i = 0; i < l; i++)
                {
                    var view = tavernOfferPrefab.CreateGO(scrollRect.content).GetComponent<TavernOfferCard>();
                
                    view.Bind(
                        dto, 
                        details.Offers[i], 
                        _adapter, 
                        uiRootView, 
                        styleResolver, 
                        portraitCache);
                    offerViews.Add(view);
                }
            }
            else
            {
                var noOffer = noOffersPrefab.CreateGO(scrollRect.content);
                noOffer.GetComponent<NoOfferPlugCard>().Bind();
            }

            scrollRect.SetSizeContentLayoutGroup(false, null, true, true);
            scrollRect.ScrollRectResetH(0);
        }
        

        private void OnPremiumRefreshClicked()
        {
            if (_adapter == null)
                return;
            
            var data = new ConfirmPaidPopupData(
                "Confirm Refresh",
                "Are you sure you want to refresh?",
                "Yes",
                $"{_adapter.GetPremiumRefreshCost()}",
                () =>
                {
                    if (_adapter.TryPremiumRefresh())
                    {
                        // Панель обновится через OnStateChanged
                    }
                },
                onClose: () => {}
            );

            ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPaidPopup, data);
        }
    }
}