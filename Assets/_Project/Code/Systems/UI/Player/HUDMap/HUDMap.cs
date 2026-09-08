using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.Progression;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.TimeWorld;
using Galactic1.Code.WorldMap;
using Galactic1.Systems;
using Galactic1.UI.Core;
using Galactic1.UI.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI.HUD
{
    public class HUDMap : UIScreenPanel
    {
        
        
        [Header("Progression")]
        [SerializeField] private TMP_Text playerLevelText;
        [SerializeField] private Image playerExperienceFillBar;
        
        [Header("World Time")] 
        [SerializeField] private SkipDayButton SkipDaysButton;
        [SerializeField] private TimeAlertView TimeAlertView;

        [SerializeField] private TMP_Text CurrentLocationText;

        [Header("Menu")] 
        [SerializeField] private GameObject settingsButton;
        [SerializeField] private GameObject gameShopButton;
        [SerializeField] private GameObject inventoryButton;
        
        
        
        private ProgressionService _progressionService;
        private EventBinding<ProgressionExperienceChangedEvent> _experienceChangedBinding;
        private EventBinding<ProgressionLevelUpEvent> _levelUpBinding;
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);
            
            // =======
            SkipDaysButton.Activate();
            TimeAlertView.Activate(container.Resolve<CampDefensePreparationService>());

            // подписываем кнопку для перехода к текущей локации
            var mapController = ServiceLocator.Current.Get<WorldMapController>();
            mapController.OnLocationChanged += _ => CurrentLocationText.text = _.Config.Header.TitleLid;
            CurrentLocationText.gameObject.RegisterButtonClick(mapController.ToCurrentLocation);
            
            
            BindButtons(container);
            
            
            // === PROGRESSION HUD ===================================================
            _progressionService = container.Resolve<ProgressionService>();

            _experienceChangedBinding = new EventBinding<ProgressionExperienceChangedEvent>(OnExperienceChanged);
            EventBus<ProgressionExperienceChangedEvent>.Register(_experienceChangedBinding);

            _levelUpBinding = new EventBinding<ProgressionLevelUpEvent>(OnLevelUp);
            EventBus<ProgressionLevelUpEvent>.Register(_levelUpBinding);

            // Save may already have Level 3 / XP 420 loaded before this HUD ever
            // existed — don't wait for the next event to show the right state.
            RefreshProgressionHUD();
            // ========================================================================
        }

        public override void Remove()
        {
            base.Remove();
            
            
            // === PROGRESSION HUD ===================================================
            EventBus<ProgressionExperienceChangedEvent>.Deregister(_experienceChangedBinding);
            EventBus<ProgressionLevelUpEvent>.Deregister(_levelUpBinding);
            // ========================================================================
        }


        /// <summary>
        /// Для регистрации действия кнопок
        /// </summary>
        void BindButtons(DIContainer container)
        {
            settingsButton.RegisterButtonClick(container.Resolve<GameSettingsSystem>().ShowWindow);
            gameShopButton.RegisterButtonClick(container.Resolve<GameStoreService>().ShowWindow);
            
            // inventory
            inventoryButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                    _ =>
                    {
                        _.GetComponent<InventoryManagementWindow>().modeController.Open(InventoryGameplayMode.Transport_SquadOnly);
                    });
            });
        }

        
        // === PROGRESSION HUD ========================================================

        private void OnExperienceChanged(ProgressionExperienceChangedEvent e)
        {
            playerLevelText.text = $"{e.CurrentLevel}";
            playerExperienceFillBar.fillAmount = e.ExperienceProgress;
        }

        private void OnLevelUp(ProgressionLevelUpEvent e)
        {
            // ExperienceProgress for the new level already arrived via the
            // ProgressionExperienceChangedEvent raised just before this one
            // (see ProgressionService.AddExperience ordering) — this handler
            // only needs to make sure the level number itself is current.
            playerLevelText.text = $"{e.NewLevel}";
        }

        private void RefreshProgressionHUD()
        {
            playerLevelText.text = $"{_progressionService.CurrentLevel}";
            playerExperienceFillBar.fillAmount = _progressionService.ExperienceProgress;
        }

        // =============================================================================
    }
}