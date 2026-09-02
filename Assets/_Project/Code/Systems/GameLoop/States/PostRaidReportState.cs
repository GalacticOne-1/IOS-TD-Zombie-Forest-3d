namespace Galactic1.Code.Systems.GameLoop.States
{
    /// <summary>
    /// Финализирует рейд и записывает его результат в GameLoopContext.
    /// UI и смена сцены здесь не происходят.
    /// </summary>
    public sealed class PostRaidReportState : GameLoopStateBase 
    {
        public override GameLoopState Id => GameLoopState.PostRaidReport;

        
        public PostRaidReportState(DIContainer container) : base(container) {}
        
        
        
        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            
            // 1. Получаем результат рейда
            var raidResult = context.Proxy.LastRaidResult;
            
            // 2. Пишем в контекст
            _context.Proxy.HasPendingRaidReport.Value = true;
            //_context.Proxy.LastCompletedState = Id;
            
            
            
            // === выход в другую сцену
            context.CurrentRaid.Scenario.ExitFromLocation();
        }


        public override void Exit(GameLoopContext context) { }
    }

}