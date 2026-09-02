
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;
using UnityEngine;

namespace Galactic1.Code.UI.Stations
{
    public class FacilityListController : UIScreenPanel, IGameService
    {
        [SerializeField] private GameObject bClose;
        [SerializeField] private StationsPanelView stationsView;
        // [SerializeField] private StoragesPanelView storagePanelView; // вкладка 2 — позже

        private StationsPanelPresenter presenter;

        private GameLoopContext gameLoopContext;
        private IConfigProvider configProvider;
        private StorageRegistry storageRegistry;

        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Register(this);
            ServiceLocator.Current.Get<TabPanelController>()
                .RegisterTab(new TabPanelController.RegistryEntry()
                {
                    Label = "facility.list",
                    Panel = this,
                    PanelId = UIScreenId.FacilityList,
                    Order = 1
                });

            gameLoopContext = container.Resolve<GameSession>().GameLoopContext;
            configProvider = container.Resolve<IConfigProvider>();
            storageRegistry = container.Resolve<StorageRegistry>();

            bClose.RegisterButtonClick(OnHide);

            gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<FacilityListController>();
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        public override void OnShow(object data = null)
        {
            base.OnShow(data);

            BindStations(); // todo
        }

        public override void OnHide()
        {
            base.OnHide();
            presenter?.Dispose();
            presenter = null;
            gameObject.SetActive(false);
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        // private void BindStations()
        // {
        //     presenter?.Dispose();
        //
        //
        //     // Все ItemConfig со станцией, отсортированные по Header.order
        //     var stationItems = GameContent.Facilities.All.Values
        //         .Where(c => c.HasModule<CraftingStationModule>())
        //         .OrderBy(c => c.Header.order)
        //         .ToList();
        //
        //     presenter = new StationsPanelPresenter(
        //         gameLoopContext,
        //         stationItems,
        //         storageRegistry,
        //         stationsView);
        //
        //     stationsView.Bind(presenter);
        // }
        private void BindStations()
        {
            presenter?.Dispose();

            var stationItems = new List<ItemConfig>();

            foreach (var facility in GameContent.Facilities.All.Values)
            {
                if (facility is CraftingStationModule)
                    stationItems.Add(facility.Item);
            }

            stationItems.Sort((a, b) => a.Header.order.CompareTo(b.Header.order));

            presenter = new StationsPanelPresenter(
                gameLoopContext,
                stationItems,
                storageRegistry,
                stationsView);

            stationsView.Bind(presenter);
        }
    }
}