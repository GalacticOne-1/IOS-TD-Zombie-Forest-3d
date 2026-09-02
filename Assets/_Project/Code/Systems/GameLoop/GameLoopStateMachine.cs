using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.GameLoop
{
    /// <summary>
    /// State Machine кор-лупа игры.
    /// Состояния ≠ сцены. Единственная точка правды для прогресса игры.
    /// Управляет переходами между состояниями Base / WorldMap / Raid / Reports.
    /// </summary>
    public class GameLoopStateMachine
    {
        /// <summary>Все состояния StateMachine</summary>
        private Dictionary<GameLoopState, IGameLoopState> _states = new();
        private GameLoopContext _context;
        
        /// <summary>Текущее состояние кор-лупа</summary>
        public IGameLoopState CurrentState { get; private set; }

        
        /// <summary>Событие для подписки UI / звука / аналитики</summary>
        public event Action<IGameLoopState> OnStateChanged;
        
        
        /// <summary>
        /// Регистрирует состояние в StateMachine.
        /// </summary>
        public void Setup(
            IEnumerable<IGameLoopState> states,
            GameLoopContext context)
        {
            _context = context;
            
            _states.Clear();
            foreach (var state in states)
                _states[state.Id] = state;
        }


        /// <summary>
        /// Переход в указанное состояние.
        /// Логика состояния выполняется внутри самого состояния.
        /// </summary>
        public void ChangeState(GameLoopState stateId)
        {
            CurrentState?.Exit(_context);
            CurrentState = _states[stateId];
            CurrentState.Enter(_context);
            OnStateChanged?.Invoke(CurrentState);
        }

        /// <summary>
        /// true - игрок в рейде
        /// </summary>
        /// <returns></returns>
        public bool IsRaidState()
            => CurrentState.Id switch
            {
                GameLoopState.RaidLaunching or GameLoopState.RaidResolving or GameLoopState.RaidInProgress => true,
                _ => false
            };

        public bool IsWorldMapState()
            => CurrentState.Id switch
            {
                GameLoopState.WorldMap => true,
                _ => false
            };
    }
}
