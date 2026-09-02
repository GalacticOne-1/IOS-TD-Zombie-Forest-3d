using Galactic1.Code.UI.Inventory;
using Galactic1.Core.UI;

namespace Galactic1.Code.Systems.GameLoop.States
{
    public sealed class CampState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.Camp;
        
        public CampState(DIContainer container) : base(container) {}

        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("CampState enter", AppConstants.show_log_core);
            
            var accessService = ServiceLocator.Current.Get<InventoryManagementWindow>().controller.AccessService;
            
            // === подключаем статы к прокси игрока
            context.RebindDisplayUnitsAfterRaid();
            new UIStatsController().Register(_container);
            
            foreach (var unit in context.PlayerUnits)
                unit.BindInventoryPreview(accessService);
            
            
        }

        public override void Exit(GameLoopContext context)
        {
            DLog.Alert("CampState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}