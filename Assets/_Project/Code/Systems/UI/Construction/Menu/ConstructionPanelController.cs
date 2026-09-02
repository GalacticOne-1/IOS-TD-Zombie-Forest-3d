using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Configs;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// UI controller панели строительства.
    /// Отображает список зданий и инициирует процесс размещения.
    /// </summary>
    public class ConstructionPanelController : UIScreenPanel, IGameService
    {
        [SerializeField] private GameObject bClose;
        [SerializeField] private ConstructionPanelView view;


        private ConstructionModeController _constructionController;
        private ConstructionRequirementService _requirementService;
        private IConfigProvider _configProvider;
        private SceneGameModeService _sceneGameModeService;
        private ConstructionConfig _constructionConfig;
        private List<FacilityModule> _facilityConfigs;
        private UIStyleResolver _uiStyleResolver;

        private ConstructionCategory _currentCategory;
        
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            _configProvider = container.Resolve<IConfigProvider>();
            _sceneGameModeService = container.Resolve<SceneGameModeService>();
            _constructionController = container.Resolve<ConstructionModeController>();
            _requirementService = container.Resolve<ConstructionRequirementService>();
            
            ServiceLocator.Current.Register(this);
            
            bClose.RegisterButtonClick(OnHide);

            _constructionConfig = _configProvider.Get<ConstructionConfig>();
            _facilityConfigs = new List<FacilityModule>(GameContent.Facilities.All.Values);

            _constructionController.OnStateChanged += Rebind;
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<ConstructionPanelController>();
        }


        
        
        public override void OnShow(object data = null)
        {
            base.OnShow(data);
            
            _sceneGameModeService.SetMode(GameModeType.Construction);
            _uiStyleResolver = ServiceLocator.Current.Get<UIStyleResolver>();
            
            gameObject.SetActive(true);

            _currentCategory = _constructionConfig.Categories[0].Category;
            Rebind();
        }

        public override void OnHide()
        {
            base.OnHide();
            gameObject.SetActive(false);
            _sceneGameModeService.SetMode(GameModeType.Normal);
        }

        void Rebind()
        {
            view.Bind(
                _container,
                _uiStyleResolver,
                _facilityConfigs,
                _constructionConfig.Categories,
                _currentCategory,
                OnFacilitySelected,
                OnTabSelected);
            
            // Форсим layout
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.TabRoot);
        }


        
        
        void OnTabSelected(ConstructionCategory category)
        {
            if (category != _currentCategory)
            {
                _currentCategory = category;
                _constructionController.ResetState();
            }
        }

        private void OnFacilitySelected(FacilityModule facility)
        {
            if (facility == null)
                return;

            var current = _constructionController.Context.BuildConfig == facility;
            _constructionController.ResetState();
            
            // если уже строим этот же объект — ничего не делаем
            if (current) return;

            _constructionController.StartPlacement(facility);
        }
    }
}