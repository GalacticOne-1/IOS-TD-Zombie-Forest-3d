using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.UI.UnitCard;
using Galactic1.Core.UI.HUD;


namespace Galactic1.Code.Systems
{
    /// <summary>
    /// Mode: использование abilities с таргетингом (гранаты, страйки)
    /// </summary>
    public sealed class AbilityTargetingGameMode : ISceneGameMode
    {
        public GameModeType ModeType => GameModeType.AbilityTargeting;

        private readonly GameTimeScaleService _time;
        private readonly CameraController _camera;
        private readonly SceneInteractionBlocker _blocker;
        private readonly ICombatTargetingService _targeting;
        private readonly AbilityTargetingHUD _hud;
        private SquadUICoordinator _squadUI;

        private TargetingRequest _request;

        public AbilityTargetingGameMode(ICombatTargetingService targeting)
        {
            _targeting = targeting;

            _time = ServiceLocator.Current.Get<GameTimeScaleService>();
            _blocker = ServiceLocator.Current.Get<SceneInteractionBlocker>();
            _camera = ServiceLocator.Current.Get<CameraController>();
        }

        public void Initialize(SquadUICoordinator squadUI)
        {
            _squadUI = squadUI;
        }

        /// <summary>
        /// Перед входом нужно прокинуть request
        /// </summary>
        public void Setup(TargetingRequest request)
        {
            _request = request;
        }

        public void Enter()
        {
            // 1. slowdown (как Aliens Dark Descent)
            _time.Set(this, GameTimeScales.AbilityTargeting);

            // 2. блок взаимодействий
            _blocker.Enable();
            _camera.Freeze.Value = true;
            
            var slot = _request.User.QuickSlot.GetSlot(
                _request.User.InventorySource.Equipment,
                _request.QuickSlotIndex);

            _squadUI.NotifyTargetingStarted(new TargetingUIData
            {
                Icon = slot.Item.Header.icon,
                ItemName = slot.Item.Header.titleLid,
                OnCancel = OnCancel
            });

            // 3. 
            _targeting.StartTargeting(_request);

#if UNITY_EDITOR
            DLog.Alert("=== Enter Ability Targeting Mode", EDlogColor.BLUE, AppConstants.show_log_core);
#endif
            
        }

        public void Exit()
        {
            // restore time
            _time.Remove(this);

            _targeting.Cancel();

            _blocker.Disable();
            _camera.Freeze.Value = false;
            _squadUI.NotifyTargetingStopped();
            
#if UNITY_EDITOR
            DLog.Alert("=== Exit Ability Targeting Mode", EDlogColor.BLUE, AppConstants.show_log_core);
#endif
        }

        private void OnCancel()
        {
            _request.OnCancel?.Invoke(); // → SetMode(Raid) в coordinator
        }
    }
}