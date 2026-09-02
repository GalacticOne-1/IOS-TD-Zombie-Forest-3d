
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.GameTime;

namespace Galactic1.Code.Systems
{
    public class RaidGameMode : ISceneGameMode
    {
        public GameModeType ModeType => GameModeType.Raid;

        public void Enter()
        {
            DLog.Alert("=== Enter Raid Game Mode", EDlogColor.BLUE, AppConstants.show_log_core);
            
            // === приводим состояние для старта рейда
            ServiceLocator.Current.Get<GameTimeScaleService>().Clear();
            ServiceLocator.Current.Get<SceneInteractionBlocker>().Disable();
        }

        public void Exit()
        {
            DLog.Alert("=== Exit Raid Game Mode", EDlogColor.BLUE, AppConstants.show_log_core);
            
        }
    }
}