using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    /// <summary>
    /// Sub-StateMachine для Tactical слоя рейда.
    /// Используется внутри RaidInProgressState для пошагового контроля событий рейда.
    /// Все состояния идентифицируются типом класса, Id не нужен.
    /// </summary>
    public class TacticalSubStateMachine
    {
        private Dictionary<Type, ITacticalState> _states;
        private DIContainer _container;
        private ITacticalState _current;
        private GameLoopContext _context;


        public ITacticalState Current => _current;


        public TacticalSubStateMachine()
        {
            DLog.Alert("TacticalSubStateMachine");
        }

        /// <summary>
        /// Инициализация Sub-StateMachine.
        /// </summary>
        /// <param name="states">Список доступных состояний</param>
        /// <param name="context">Общий GameLoopContext для всех состояний</param>
        public void Setup(
            DIContainer container,
            GameLoopContext context, 
            IEnumerable<ITacticalState> states)
        {
            _container = container;
            _context = context;
            
            _states = new Dictionary<Type, ITacticalState>();
            foreach (var state in states)
                _states[state.GetType()] = state;

        }

        /// <summary>
        /// Переход в новое состояние по типу класса.
        /// </summary>
        /// <typeparam name="T">Тип состояния, реализующий ITacticalState</typeparam>
        public void ChangeState<T>() where T : ITacticalState
        {
            _current?.Exit(_context);

            if (_states.TryGetValue(typeof(T), out var next))
            {
                _current = next;
                _current.Enter(_container, _context);
            }
            else
            {
                throw new Exception($"Tactical state {typeof(T)} not found in Sub-StateMachine!");
            }
        }
        
        /// <summary>
        /// Переход в новое состояние по типу класса.
        /// </summary>
        public void ChangeState(Type stateType)
        {
            _current?.Exit(_context);

            if (_states.TryGetValue(stateType, out var next))
            {
                _current = next;
                _current.Enter(_container, _context);
            }
            else
            {
                throw new Exception($"Tactical state {stateType} not found in Sub-StateMachine!");
            }
        }

        /// <summary>
        /// Обновление текущего состояния. Вызывается каждый кадр из RaidInProgressState.Update.
        /// </summary>
        /// <param name="deltaTime">Время кадра</param>
        public void Update(float deltaTime)
        {
            _current?.Update(_context, deltaTime);
        }
    }

    
}
