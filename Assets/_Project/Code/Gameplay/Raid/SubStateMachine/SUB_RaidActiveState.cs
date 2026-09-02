using Galactic1.Code.Gameplay.RaidDirector;

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    // =========================================================
    // Активный ход рейда (Tactical)
    // =========================================================
    public sealed class SUB_RaidActiveState : ITacticalState
    {
        private readonly DIContainer _container;
        private RaidDirectorRuntime _raidDirectorRuntime;

        public SUB_RaidActiveState(DIContainer container)
        {
            _container = container;
        }

        public void Enter(DIContainer container, GameLoopContext context)
        {
            DLog.Alert("RaidActiveState enter: симуляция боя", AppConstants.show_log_core);
            
            
            context.CurrentRaid.Scenario.OnBattleStarted();
            
            // Здесь может запускаться симуляция ходов юнитов

            _raidDirectorRuntime = _container.Resolve<RaidDirectorRuntime>();
        }

        public void Update(GameLoopContext context, float deltaTime)
        {
            // Проверяем завершение рейда
            // if (context.CurrentRaid.IsFinished)
            // {
            //     context.SubStateMachine.ChangeState<RaidEventsState>();
            // }
            
            _raidDirectorRuntime?.Tick(deltaTime);
        }

        public void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidActiveState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}