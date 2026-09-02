

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    // =========================================================
    // Проверка выполнения целей рейда
    // =========================================================
    public sealed class SUB_RaidCheckObjectivesState : ITacticalState
    {
        public void Enter(DIContainer container, GameLoopContext context)
        {
            DLog.Alert("RaidCheckObjectivesState enter: проверка целей рейда", AppConstants.show_log_core);
            
            var raid = context.CurrentRaid;

            // if (raid.Objectives.AllPrimaryCompleted)
            //     raid.EndReason = RaidEndReason.ObjectivesCompleted;
            // else if (!raid.Squad.HasAliveUnits)
            //     raid.EndReason = RaidEndReason.SquadWiped;
            // else
            //     raid.EndReason = RaidEndReason.PanicAbort;
            
            
            // Переход к финальной очистке
            context.TacticalStateMachine.ChangeState<SUB_RaidCleanupState>();
        }

        public void Update(GameLoopContext context, float deltaTime)
        {
            // Переход к финальной очистке
            //context.TacticalStateMachine.ChangeState<SUB_RaidCleanupState>();
        }

        public void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidCheckObjectivesState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}