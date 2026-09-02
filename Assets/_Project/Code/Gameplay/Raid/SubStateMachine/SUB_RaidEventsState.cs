namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    // =========================================================
    // Обработка событий рейда (триггеры, лут, эффекты)
    // =========================================================
    public sealed class SUB_RaidEventsState : ITacticalState
    {
        public void Enter(DIContainer container, GameLoopContext context)
        {
            DLog.Alert("RaidEventsState enter: обработка событий рейда", AppConstants.show_log_core);
            // Распределяем события после завершения активной фазы
        }

        public void Update(GameLoopContext context, float deltaTime)
        {
            // Завершение обработки событий
            context.TacticalStateMachine.ChangeState<SUB_RaidCheckObjectivesState>();
        }

        public void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidEventsState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}