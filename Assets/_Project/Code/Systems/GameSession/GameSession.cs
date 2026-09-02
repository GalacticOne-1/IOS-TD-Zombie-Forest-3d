using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Core.Systems.GameLoopSession
{
    /// <summary>
    /// Игровая сессия.
    /// Живёт между сценами.
    /// Хранит состояние кор-лупа и времени.
    /// </summary>
    public sealed class GameSession : IGameService // создается в CoreRegistrations после JsonGameStateProvider
    {
        public GameLoopContext GameLoopContext { get; private set; }
        public StrategicSquadSystem StrategicSquadSystem { get; private set; }
        
        
        #region SAVING

        private bool _isDirty;

        public void MarkDirty()
        {
            _isDirty = true;
        }

        public void SaveIfDirty()
        {
            if (!_isDirty)
                return;

            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
            _isDirty = false;
        }

        #endregion
        
        
        
        
        public void Initialize(DIContainer container)
        {
            // 1️⃣ Создаём Runtime контекст
            GameLoopContext = new GameLoopContext(
                container.Resolve<IGameStateProvider>().GameStateProxy.GameLoopContext,
                container.Resolve<GameLoopStateMachine>());
            
            // 2️⃣ Активируем Live Sync, чтобы подписки начали работать
            GameLoopContext.Proxy.ActivateLiveSync();
            
            StrategicSquadSystem = new StrategicSquadSystem(GameLoopContext);
            container.RegisterInstance(new SquadValidationService(GameLoopContext));
            ServiceLocator.Current.Register(container.Resolve<SquadValidationService>());
        }


        /// <summary>
        /// Подписываемся для изменения статов
        /// </summary>
        /// <param name="controller"></param>
        public void UnitStatsRegister(InventoryManagementController controller)
        {
            controller.OnUnitChanged += _ =>
            {
                var units = GameLoopContext.GetDisplayAllUnit();

                var i = 0;
                foreach (var unit in units)
                {
                    if (unit != null && _ == unit.Id)
                    {
                        unit.Stats.PushAllStats();
                        break;
                    }

                    i++;
                }
            };
        }
    }
}