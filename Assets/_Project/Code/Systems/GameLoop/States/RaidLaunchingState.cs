namespace Galactic1.Code.Systems.GameLoop.States
{
    public sealed class RaidLaunchingState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.RaidLaunching;
        

        
        public RaidLaunchingState(DIContainer container) : base(container) {}
        

        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("RaidLaunchingState enter", AppConstants.show_log_core);
            //_launcher.Launch(context.CurrentRaid);
            //_sm.ChangeState(GameLoopState.RaidInProgress);
        }

        public override void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidLaunchingState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }

}