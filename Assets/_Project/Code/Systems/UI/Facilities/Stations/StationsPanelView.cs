
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.BuildingPanel;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Stations
{
    public sealed class StationsPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private ScrollRect scrollRect;

        private readonly List<StationCardView> pool = new();
        private StationsPanelPresenter presenter;

        private void OnDestroy() => presenter?.Dispose();

        public void Bind(StationsPanelPresenter stationsPresenter)
        {
            presenter = stationsPresenter;
            presenter.Open();
        }

        public void Render(UIStyleResolver styleResolver, IReadOnlyList<StationCardDTO> cards)
        {
            bool y = false;
            while (pool.Count < cards.Count)
            {
                var go = Instantiate(cardPrefab, scrollRect.content);
                var card = go.GetComponent<StationCardView>();
                pool.Add(card);
                y = true;
            }
            if(y)
                scrollRect.SetSizeContentGridLayoutGroup(true, false, true);

            
            for (int i = 0; i < pool.Count; i++)
            {
                bool active = i < cards.Count;
                pool[i].gameObject.SetActive(active);
                if (active)
                    pool[i].Render(styleResolver, cards[i], OnCardClicked);
            }
            
            scrollRect.ScrollRectResetV();
        }

        void OnCardClicked(RuntimeId stationId)
        {
            ServiceLocator.Current
                .Get<FacilityPanelController>()
                .OpenByConfigId(stationId);
        }

        void OnBuildClicked(string stationId)
        {
            
        }
    }
}