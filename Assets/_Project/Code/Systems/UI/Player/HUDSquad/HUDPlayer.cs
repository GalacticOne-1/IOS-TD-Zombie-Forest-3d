
using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Interaction;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.UnitCard;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Core.UI.HUD
{
    public class HUDPlayer : UIScreenPanel
    {
        [field: Header("Controls")] 
        [SerializeField] private Transform unitCardRoot;
        [SerializeField] private GameObject ButtonSquadFocus;
        [SerializeField] private AbilityTargetingHUD abilityTargetingHUD;
        //public UIJoystick joystick;
        //[field: SerializeField] public TargetHPBarUI targetHPBar { get; private set; }
        
       
        //[field: SerializeField] public GameObject jump { get; private set; }
        //[field: SerializeField] public UIButtonPocket quickButton1 { get; private set; }
       // [field: SerializeField] public UIButtonPocket quickButton2 { get; private set; }
       // [field: SerializeField] public UIButtonAttack attackButton { get; private set; }
       // [field: SerializeField] public UIButtonAction actionButton { get; private set; }


        //private HUDSlotsController _hudSlotsController;
        //private EventBinding<SceneClearEvent> onSwitchClear;
        
        private UnitCardBindingSystem _cardBindings;
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);
            
           // _hudSlotsController = GetComponent<HUDSlotsController>();
            
            BindButtons();
            
            // register buttons
            //jump.RegisterButtonClick(JoystickController.I.Jump);

            
            
            // === когда отряд заспавнен создаем карточки
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(
                () => BindSquad(container.Resolve<Systems.GameLoopSession.GameSession>().GameLoopContext.CurrentRaid.Squad)));
        }

        public override void Remove()
        {
            base.Remove();
            
            _cardBindings?.Dispose();
            unitCardRoot.MakeEmpty();
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

        
        public void ConnectToInventory()
        {
            // Определяем, кто сейчас под управлением
            // var IsDragon = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy
            //     .PlayerUnitData[0].OnDragon.CurrentValue;
            //
            // _hudSlotsController.Unbind();
            //
            // if (!IsDragon)
            // {
            //     ApplyHUDProfile(playerHUD);
            //     _hudSlotsController.Bind(
            //         this, 
            //         ServiceLocator.Current.Get<InventoryRepository>().PlayerEquipment
            //     );
            // }
            // else
            // {
            //     ApplyHUDProfile(dragonHUD);
            //     _hudSlotsController.Bind(
            //         this,
            //         ServiceLocator.Current.Get<InventoryRepository>().DragonEquipment
            //     );
            // }
        }
        
        private void ApplyHUDProfile(HUDProfile profile)
        {
            // attackButton.gameObject.SetActive(profile.showAttack);
            // actionButton.gameObject.SetActive(profile.showAction);
            // jump.SetActive(profile.showJump);
            // dragonButton.SetActive(profile.showDragonButton);
            // quickButton1.gameObject.SetActive(profile.showQuick1);
            // quickButton2.gameObject.SetActive(profile.showQuick2);
        }
        
        
        
    }
}