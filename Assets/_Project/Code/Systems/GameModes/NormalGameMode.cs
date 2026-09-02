namespace Galactic1.Code.Systems
{
    /// <summary>
    /// Default gameplay mode.
    /// Player can interact with units, buildings and UI.
    /// </summary>
    public class NormalGameMode : ISceneGameMode
    {
        public GameModeType ModeType => GameModeType.Normal;

        public void Enter()
        {
            DLog.Alert("=== Enter Normal Game Mode", EDlogColor.BLUE, AppConstants.show_log_core);
            // enable interactions
            // enable building selection
            // enable camera normal controls
        }

        public void Exit()
        {
            DLog.Alert("=== Exit Normal Game Mode", EDlogColor.BLUE, AppConstants.show_log_core);
            // nothing special
        }
    }
}