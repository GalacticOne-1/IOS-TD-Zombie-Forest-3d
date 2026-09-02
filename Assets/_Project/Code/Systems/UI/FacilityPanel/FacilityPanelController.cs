using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Buildings;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.UI.Buildings;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;

namespace Galactic1.Code.UI.BuildingPanel
{
    public class FacilityPanelController : UIScreenPanel, IGameService
    {
        [SerializeField] private GameObject bClose;
        [SerializeField] private FacilityPanelView view;


        private GameLoopContext _gameLoopContext;
        private ISceneAdapterFactory _factory;
        private FacilityPresentationAdapter _presentation;
        
        private FacilityUpgradePanelModule _upgradeModule;
        private CommonActionsPanelModule _actionsModule;
        
        private BaseCampFacilityRuntime _runtime;  
        private IFacilitySceneAdapter _adapter;
        private IFacilitySceneAdapter _upgrade;
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Register(this);
            ServiceLocator.Current.Get<TabPanelController>()
                .RegisterTab(new TabPanelController.RegistryEntry()
                {
                    Label = "facility.panel",
                    Panel = this,
                    PanelId = UIScreenId.FacilityPanel,
                    Order = 0
                });

            _gameLoopContext = container.Resolve<GameSession>().GameLoopContext;
            _factory = container.Resolve<ISceneAdapterFactory>();
            _presentation = container.Resolve<FacilityPresentationAdapter>();
            
            view.Prewarm();
            bClose.RegisterButtonClick(OnHide);
            
            
            // === общие модули
            _upgradeModule = view.UpgradeModule;
            _actionsModule = view.CommonActionsModule;
            _actionsModule.OnUpgradeRequested += HandleUpgradeRequested;
            
            //gameObject.SetActive(true);
            OnHide();
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<FacilityPanelController>();
        }
        
        
        public void OpenRuntime(BaseCampFacilityRuntime runtime)
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.FacilityPanel,
                null,
                _ => BindRuntime(runtime));
        }
        

        /// <summary>
        /// Открытие из сцены (клик на здание) — кнопка Back не нужна.
        /// </summary>
        public void Open(FacilityInstance facility)
        {

            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.FacilityPanel,
                null,
                _ => BindBuilding(facility));
        }
        
        /// <summary>
        /// Открытие из FacilityListController по ConfigId.
        /// Показывает кнопку возврата в список.
        /// </summary>
        public void OpenByConfigId(RuntimeId configId)
        {
            // Ищем FacilityInstance в репозитории по ConfigId
            var repo = ServiceLocator.Current.Get<BaseFacilityRepository>();
            FacilityInstance target = null;

            foreach (var kvp in repo.All)
            {
                if (kvp.Value.ItemConfig?.Id == configId)
                {
                    target = kvp.Value;
                    break;
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"[FacilityPanelController] Instance not found for configId: {configId}");
                return;
            }


            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.FacilityPanel,
                null,
                _ => BindBuilding(target));
        }

        public override void OnHide()
        {
            base.OnHide();
            view.Hide();
            Unbind();
        }


        
        // private void BindBuilding(FacilityInstance facility)
        // {
        //     Unbind();
        //     
        //     if (!_gameLoopContext.TryGetBuilding(facility.UniqueId, out var runtime))
        //         return;
        //
        //     _runtime = runtime;
        //      var a = _factory.Create(runtime);
        //      _adapter = a.adapter;
        //      _upgrade = a.upgrade;
        //
        //     if (_adapter == null)
        //         return;
        //
        //     _adapter.OnStateChanged += Refresh;
        //
        //     if (_upgrade != null)
        //         _upgrade.OnStateChanged += Refresh;
        //
        //     var dto = _presentation.Create(_runtime);
        //     view.Bind(dto, _adapter, a.upgrade);
        // }
        
        private void BindBuilding(FacilityInstance facility)
        {
            if (!_gameLoopContext.TryGetBuilding(facility.UniqueId, out var runtime))
                return;

            BindRuntime(runtime);
        }
        

        private void BindRuntime(BaseCampFacilityRuntime runtime)
        {
            Unbind();

            _runtime = runtime;

            var a = _factory.Create(runtime);
            _adapter = a.adapter;
            _upgrade = a.upgrade;

            if (_adapter == null)
                return;

            _adapter.OnStateChanged += Refresh;

            if (_upgrade != null)
                _upgrade.OnStateChanged += Refresh;

            var dto = _presentation.Create(runtime);
            view.Bind(dto, _adapter, a.upgrade);
        }
        
        private void Unbind()
        {
            if (_adapter != null)
                _adapter.OnStateChanged -= Refresh;
            
            if (_upgrade != null)
                _upgrade.OnStateChanged -= Refresh;

            _adapter = null;
            _runtime = null;
        }
        
        private void Refresh()
        {
            var dto = _presentation.Create(_runtime);
            view.Rebind(dto);
        }

        
        
        
        private void HandleUpgradeRequested()
        {
            if (_upgradeModule == null)
                return;

            _upgradeModule.Show();
        }
    }

}