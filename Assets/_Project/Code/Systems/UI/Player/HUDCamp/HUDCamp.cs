using Galactic1.Code.Gameplay.WorldThreatConfig;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Progression;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.Construction;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.Stations;
using Galactic1.Code.UI.TimeWorld;
using Galactic1.Configs;
using Galactic1.Core.Results;
using Galactic1.Systems;
using Galactic1.UI.Core;
using Galactic1.UI.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI.HUD
{
    public class HUDCamp : UIScreenPanel
    {
        
        [Header("Progression")]
        [SerializeField] private TMP_Text playerLevelText;
        [SerializeField] private Image playerExperienceFillBar;
        
        
        [Header("World Time")] [SerializeField]
        private SkipDayButton skipDaysButton;

        [SerializeField] private TimeAlertView timeAlertView;
        [SerializeField] private TMP_Text campCapacityText;
        [SerializeField] private GameObject defenseCampButton;
        [SerializeField] private GameObject defenseCampButtonTest;

        [Header("Menu")] 
        [SerializeField] private GameObject settingsButton;
        [SerializeField] private GameObject facilitiesButton;
        [SerializeField] private GameObject gameShopButton;
        [SerializeField] private GameObject worldMapButton;
        [SerializeField] private GameObject constructionButton;
        [SerializeField] private GameObject inventoryButton;

        private GameLoopContext gameLoopContext;
        private ProgressionService _progressionService;
        private ICampCapacityService capacityService;
        private CampDefensePreparationService campDefensePreparationService;
        private SquadValidationService _squadValidation;
        
        private EventBinding<ProgressionExperienceChangedEvent> _experienceChangedBinding;
        private EventBinding<ProgressionLevelUpEvent> _levelUpBinding;
        
        
        

        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);

            // =======
            gameLoopContext = container.Resolve<Systems.GameLoopSession.GameSession>().GameLoopContext;
            capacityService = container.Resolve<ICampCapacityService>();
            campDefensePreparationService = container.Resolve<CampDefensePreparationService>();
            _squadValidation = container.Resolve<SquadValidationService>();

            skipDaysButton.Activate();
            timeAlertView.Activate(campDefensePreparationService);

            // HUD ничего не знает про Threat — только вызывает сервис и
            // отражает его состояние.
            defenseCampButton.RegisterButtonClick(() => campDefensePreparationService.StartDefense());
            defenseCampButtonTest.RegisterButtonClick(() => campDefensePreparationService.StartDefense());
            campDefensePreparationService.DefenseAvailabilityChanged += OnDefenseAvailabilityChanged;

            // восстанавливаем состояние UI после загрузки сцены
            Refresh();

            // === слоты для бойцов
            CapacityRefresh();
            gameLoopContext.OnUnitChanged += CapacityRefresh;
            capacityService.OnCapacityChanged += CapacityRefresh;

            // регистрация действия кнопок
            settingsButton.RegisterButtonClick(container.Resolve<GameSettingsSystem>().ShowWindow);
            gameShopButton.RegisterButtonClick(container.Resolve<GameStoreService>().ShowWindow);
            worldMapButton.RegisterButtonClick(() =>
            {
                switch (_squadValidation.ValidateForWorldMap())
                {
                    case SquadValidationResult.Success:
                        EventBus<WorldMapSceneRequestEvent>.Raise(new WorldMapSceneRequestEvent());
                        break;
                    case SquadValidationResult.EmptySquad:
                        ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.SquadIsEmpty);
                        break;
                }
            });

            facilitiesButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.FacilityList, null,
                    _ => { _.GetComponent<FacilityListController>().OnShow(); });
            });

            constructionButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.BaseConstructionMenu, null,
                    _ => { _.GetComponent<ConstructionPanelController>().OnShow(); });
            });

            inventoryButton.RegisterButtonClick(() =>
            {
                ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                    _ =>
                    {
                        _.GetComponent<InventoryManagementWindow>().modeController
                            .Open(InventoryGameplayMode.Camp_AllUnits);
                    });
            });
            
            
            
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

            gameLoopContext.OnUnitChanged -= CapacityRefresh;
            capacityService.OnCapacityChanged -= CapacityRefresh;
            campDefensePreparationService.DefenseAvailabilityChanged -= OnDefenseAvailabilityChanged;

            // === PROGRESSION HUD ===================================================
            EventBus<ProgressionExperienceChangedEvent>.Deregister(_experienceChangedBinding);
            EventBus<ProgressionLevelUpEvent>.Deregister(_levelUpBinding);
            // ========================================================================
        }

        void CapacityRefresh()
        {
            campCapacityText.text = $"{capacityService.GetCurrentUnits()}/{capacityService.GetMaxCapacity()}";
        }

        /// <summary>
        /// Восстанавливает состояние кнопок по текущему значению сервиса.
        /// Вызывается при открытии HUD (после загрузки сцены).
        /// </summary>
        private void Refresh()
        {
            ApplyDefenseAvailability(campDefensePreparationService.IsDefenseAvailable);
        }

        private void OnDefenseAvailabilityChanged(bool isAvailable)
        {
            ApplyDefenseAvailability(isAvailable);
        }

        private void ApplyDefenseAvailability(bool isAvailable)
        {
            defenseCampButton.SetActive(isAvailable);
            skipDaysButton.gameObject.SetActive(!isAvailable);
            skipDaysButton.DayBarRoot.SetActive(!isAvailable);
            
            // todo
            // for dev: что бы кнопки всегда были активны
            var config = ServiceLocator.Current.Get<ConfigProvider>().Get<WorldThreatConfig>();
            if (config.TestThreat)
            {
                defenseCampButtonTest.SetActive(true);
                defenseCampButton.SetActive(false);
                skipDaysButton.gameObject.SetActive(true);
                skipDaysButton.DayBarRoot.SetActive(true);
            }
            else
            {
                defenseCampButtonTest.SetActive(false);
            }
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