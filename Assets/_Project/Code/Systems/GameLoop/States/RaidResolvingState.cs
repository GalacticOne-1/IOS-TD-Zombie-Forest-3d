using System.Collections.Generic;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.GameLoop.States
{
    public sealed class RaidResolvingState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.RaidResolving;
        
        

        public RaidResolvingState(DIContainer container) : base(container) {}
        
        

        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("RaidResolvingState enter", AppConstants.show_log_core);
            
            var raid = context.CurrentRaid;

            // === применяем состояние юнитов, если это не отладка
            if (raid.MissionResult.EndReason != RaidEndReason.DebugCancel)
            {
                ApplyResults(context.CurrentRaid, context.PlayerUnits, context.PlayerTransport);
                raid.Scenario.ApplyResults();
            }
            
            // === очищаем зомби сервисы
            //context.CurrentRaid.CurrentRaidLifecycle?.Dispose();
            //context.CurrentRaid.CurrentRaidLifecycle = null;
            
            
            // ***************************
            _container.Resolve<GameLoopStateMachine>().ChangeState(GameLoopState.PostRaidReport);
        }

        public override void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidResolvingState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
        
        
        public static void ApplyResults(
            RaidRuntime raid, 
            IReadOnlyCollection<UnitRuntime> units,
            TransportRuntime transport)
        {
            // 🔹 юниты
            RaidResolvingPipeline.Resolve(
                raid.Squad.Units,
                units,
                u => u.Proxy.Id
            );

            // 🔹 транспорт (один)
            RaidResolvingPipeline.Resolve(
                raid.PlayerTransport,
                transport,
                t => t.Proxy.Id
            );
        }
    }

}