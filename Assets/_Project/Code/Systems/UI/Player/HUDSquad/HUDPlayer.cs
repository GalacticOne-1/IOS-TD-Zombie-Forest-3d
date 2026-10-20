using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Progression;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Interaction;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.UnitCard;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI.HUD
{
    public class HUDPlayer : UIScreenPanel
    {
        [field: Header("Controls")] 
        [SerializeField] private Transform unitCardRoot;
        [SerializeField] private GameObject ButtonSquadFocus;
        [SerializeField] private AbilityTargetingHUD abilityTargetingHUD;
        [SerializeField] private UIDamageFeedback _uiDamageFeedback;
        

        [Header("Progression")]
        [SerializeField] private TMP_Text playerLevelText;
        [SerializeField] private Image playerExperienceFillBar;

       
        public UIDamageFeedback UIDamageFeedback => _uiDamageFeedback;

        private UnitCardBindingSystem _cardBindings;

        private ProgressionService _progressionService;
        private EventBinding<ProgressionExperienceChangedEvent> _experienceChangedBinding;
        private EventBinding<ProgressionLevelUpEvent> _levelUpBinding;
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);
            
            BindButtons();
            
            
            // === когда отряд заспавнен создаем карточки
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
                BindSquad(container.Resolve<Systems.GameLoopSession.GameSession>()
                    .GameLoopContext.CurrentRaid.Squad)));
            
            

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
            
            _cardBindings?.Dispose();
            unitCardRoot.MakeEmpty();

            // === PROGRESSION HUD ===================================================
            EventBus<ProgressionExperienceChangedEvent>.Deregister(_experienceChangedBinding);
            EventBus<ProgressionLevelUpEvent>.Deregister(_levelUpBinding);
            // ========================================================================
        }


        /// <summary>
        /// Для регистрации действия кнопок
        /// </summary>
        void BindButtons()
        {
            ButtonSquadFocus.RegisterButtonClick(ServiceLocator.Current.Get<CameraController>().FocusOnSquad);
        }
        
        
        public void BindSquad(SquadRuntime squad)
        {
            _cardBindings?.Dispose();
            
            // создаем карточки
            var prefab = Resources.Load<UnitCardView>($"{AppConstants.PATH_UI_GAMEPLAY}HUD/HUD_unit_card");
            List<UnitCardView> cards = new List<UnitCardView>();
            var l = squad.Units.Count;
            for (int i = 0; i < l; i++)
                cards.Add(Instantiate(prefab, unitCardRoot));
            
            
            _cardBindings = new UnitCardBindingSystem();
            _cardBindings.Bind(
                squad, 
                cards,
                _container.Resolve<ItemUseService>(),
                _container.Resolve<AbilityUseCoordinator>(),
                abilityTargetingHUD,
                _container.Resolve<SceneGameModeService>(),
                _container.Resolve<UIStateController>(),
                ServiceLocator.Current.Get<InventoryManagementWindow>().modeController,
                _container.Resolve<UIInputRouter>()
            );
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