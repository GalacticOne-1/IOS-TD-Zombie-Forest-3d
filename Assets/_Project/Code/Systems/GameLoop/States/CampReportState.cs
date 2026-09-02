using Galactic1.Code.UI.CampDefenseReport;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.GameLoop.States
{
    public sealed class CampReportState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.CampReport;
        
        private WidgetQueueService widgetQueue;


        public CampReportState(DIContainer container) : base(container)
        {
            widgetQueue = container.Resolve<WidgetQueueService>();
        }


        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("CampReportState enter", AppConstants.show_log_core);
            
            /*
             *  Запускаем очередь панелей после рейда
             */
            if (context.Proxy.HasPendingRaidReport.Value)
            {
                // === отмечаем показ отчета
                context.Proxy.HasPendingRaidReport.Value = false;
                _container.Resolve<IGameStateProvider>().SaveGameState();
                
                widgetQueue.Enqueue(new WidgetRequest
                {
                    Priority = 10,
                    ScreenId = UIScreenId.CampDefenseReport,
                    OnShow = onDone =>
                    {
                        // смена состояния желательно должна быть из последнего окна в очереди виджета !!!
                        onDone += () =>
                        {
                            // очищаем убитых юнитов 
                            context.CleanupDeadUnitsAfterRaid();
                            
                            // переводим в обычное состояние лагеря
                            _container.Resolve<GameLoopStateMachine>().ChangeState(GameLoopState.Camp);
                            
                            // после закрытия отчета очищаем рейд
                            context.CurrentRaid = null;
                        };
                        new CampDefenseReportFlowController(context, UIScreenId.CampDefenseReport)
                            .StartFlow(
                                context.CurrentRaid,
                                context.Proxy.LastRaidResult,
                                onDone
                            );
                    }
                });
                
                
                
                
                // =========================
                widgetQueue.StartShow();
            }
        }


        public override void Exit(GameLoopContext context) { }
    }

}