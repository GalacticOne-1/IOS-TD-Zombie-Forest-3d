using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Runtime.Trader;
using Galactic1.Game.UI.Buildings.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// UI-модуль панели трейдера (покупка предметов).
    /// </summary>
    public class CampTraderPanelModule : FacilityPanelModule
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject traderOfferPrefab;

        private List<CampTraderOfferCard> offerViews = new();

        private CampTraderSceneAdapter _adapter;


        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.CampTrader;

        public override void Bind(
            FacilityDTO dto,
            object sceneAdapter = null,
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);
            _adapter = sceneAdapter as CampTraderSceneAdapter;

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

            var details = dto.Details as CampTraderDetailsDTO;
            if (details == null)
                return;

            // === offers list
            var l = details.Offers.Count;
            for (int i = 0; i < l; i++)
            {
                var view = traderOfferPrefab.CreateGO(scrollRect.content).GetComponent<CampTraderOfferCard>();

                view.Bind(
                    details.Offers[i],
                    _adapter);

                offerViews.Add(view);
            }

            scrollRect.SetSizeContentLayoutGroup(false, null, true, true);
            scrollRect.ScrollRectResetH(0);
        }
    }
}
