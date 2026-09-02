
using System;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.UI.CampDefenseReport;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.RaidReport;
using Galactic1.Code.WorldMap;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Core.UI;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.GameLoop.States
{
    /// <summary>
    /// Состояние кор-лупа: выбор локации для рейда на глобальной карте.
    /// Игрок видит карту, может выбрать локацию.
    /// Логика: проверка доступности, расчет стоимости визита, подготовка TravelData.
    /// </summary>
    public class WorldMapState : GameLoopStateBase
    {
        public override GameLoopState Id => GameLoopState.WorldMap;

        private WidgetQueueService widgetQueue;
        private readonly WorldMapController _worldMapController;
        private EventBinding<HordeAttackMissedEvent> hordeMissedEventBinding;

        public WorldMapState(DIContainer container, WorldMapController worldMapController) : base(container)
        {
            widgetQueue = container.Resolve<WidgetQueueService>();
            _worldMapController = worldMapController;


            // === подписка на событие пропуска орды во время перемещения по карте
            // отписка при смене сцены
            hordeMissedEventBinding = new EventBinding<HordeAttackMissedEvent>(ReportMissedHorde);
            EventBus<HordeAttackMissedEvent>.Register(hordeMissedEventBinding);
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                EventBus<HordeAttackMissedEvent>.Deregister(hordeMissedEventBinding);
            }));
        }


        public override void Enter(GameLoopContext context)
        {
            base.Enter(context);
            DLog.Alert("WorldMapState enter", AppConstants.show_log_core);
            
            var accessService = ServiceLocator.Current.Get<InventoryManagementWindow>().controller.AccessService;
            
            // === подключаем статы к прокси игрока
            context.RebindDisplayUnitsAfterRaid();
            new UIStatsController().Register(_container);
            
            foreach (var unit in context.PlayerUnits)
                unit.BindInventoryPreview(accessService);

            /*
             *  Запускаем очередь панелей после рейда
             */
            if (context.Proxy.HasPendingRaidReport.Value)
            {
                // === отмечаем показ отчета
                context.Proxy.HasPendingRaidReport.Value = false;
                _container.Resolve<IGameStateProvider>().SaveGameState();

                var mapNode = ServiceLocator.Current.Get<WorldMapController>().GetNode(context.CurrentRaid.Id);

                // #1 raid report
                widgetQueue.Enqueue(new WidgetRequest
                {
                    Priority = 10,
                    ScreenId = UIScreenId.RaidReport,
                    OnShow = onDone =>
                    {
                        onDone += () =>
                        {
                            // после закрытия отчета очищаем рейд
                            context.CurrentRaid = null;
                        };
                        ServiceLocator.Current.Get<RaidReportFlowController>().StartFlow(
                            mapNode.Config.Header.TitleLid,
                            context.CurrentRaid,
                            context.Proxy.LastRaidResult,
                            onDone
                        );
                    }
                });


                // #9 review
                if (ServiceLocator.Current.Get<Review>().NeedRequest())
                {
                    widgetQueue.Enqueue(new WidgetRequest
                    {
                        Priority = 0,
                        ScreenId = UIScreenId.Review,
                        OnShow = onDone =>
                        {
                            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                                UIScreenId.Review,
                                null,
                                _ =>
                                {
                                    var screen = _.GetComponent<Review>();
                                    screen.OnClosed += onDone;
                                    screen.OnShow();

                                });
                        }
                    });
                }

                widgetQueue.StartShow();
                // ===

            }

        }

        public override void Exit(GameLoopContext context)
        {
            DLog.Alert("WorldMapState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }



        void ReportMissedHorde()
        {
            Debug.Log($"Report Missed Horde");

            var gameLoopContext = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            
            // #1 сбрасываем режим защиты в лагере что бы не было бага
            _container.Resolve<CampDefensePreparationService>().CompleteDefense();
            
            
            // ====================================================================================================
            // #2 запускаем рапорт о штрафе за пропуск орды
            
            // смена состояния желательно должна быть из последнего окна в очереди виджета !!!
            Action onDone = () =>
            {
                // очищаем убитых юнитов 
                gameLoopContext.CleanupDeadUnitsAfterRaid();
                            
                // переводим в обычное состояние лагеря
                //_container.Resolve<GameLoopStateMachine>().ChangeState(GameLoopState.WorldMap);
                            
                // после закрытия отчета очищаем рейд
                gameLoopContext.CurrentRaid = null;
            };
            new CampDefenseReportFlowController(gameLoopContext, UIScreenId.CampDefenseMapReport)
                .StartFlow(
                    gameLoopContext.CurrentRaid,
                    gameLoopContext.Proxy.LastRaidResult,
                    onDone
                );
        }
    }
}