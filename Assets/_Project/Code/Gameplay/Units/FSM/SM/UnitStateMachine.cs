using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// FSM юнита. Изменения относительно оригинала:
    ///   — Добавлено событие OnStateChanged, на которое подписывается IUnitBrain.
    ///   — unit теперь типизирован как UnitInstance (базовый), а не SurvivorInstance.
    ///   — Всё остальное — без изменений.
    /// </summary>
    public sealed class UnitStateMachine
    {
        private readonly Dictionary<UnitStateId, IUnitState> _states = new();
        private IUnitState _current;
        private UnitInstance _unit;

        private static readonly HashSet<UnitStateId> _alwaysForced = new()
        {
            UnitStateId.Dying,
            UnitStateId.Dead,
            UnitStateId.Panicking,
            UnitStateId.Suppressed,
        };

        public UnitStateId CurrentStateId => _current?.StateId ?? UnitStateId.Idle;

        private readonly Stack<(IUnitState state, IUnitCommand command)> _stateStack = new();
        private IUnitCommand _currentCommand;

        public bool HasStack => _stateStack.Count > 0;

        /// <summary>
        /// Срабатывает после каждого успешного перехода в новое состояние.
        /// IUnitBrain подписывается, чтобы разблокировать CommandLock и т.п.
        /// </summary>
        public event Action<UnitStateId> OnStateChanged;

        // ── Инициализация ──────────────────────────────────────────────

        public void Initialize(
            UnitInstance unit, // ← был SurvivorInstance
            Dictionary<UnitStateId, IUnitState> states,
            UnitStateId initialState)
        {
            _unit = unit;
            foreach (var kv in states)
                _states[kv.Key] = kv.Value;

            ForceState(initialState);
        }

        // ── Tick ───────────────────────────────────────────────────────

        public void Tick(float dt) => _current?.Tick(_unit, dt);

        // ── Execute ────────────────────────────────────────────────────

        public void Execute(IUnitCommand command)
        {
            if (command == null) return;


#if UNITY_EDITOR
            DLog.Alert($"({_unit.name}) [FSM] Execute {command.GetType().Name} | current={CurrentStateId}",
                AppConstants.show_log_unit_fsm);
#endif
            
            if (_alwaysForced.Contains(command.TargetState))
            {
                ForceState(command.TargetState, command);
                return;
            }

            bool commandPending = _current?.HandleCommand(_unit, command) ?? false;

            if (commandPending) return;

            if (!command.CanExecute(CurrentStateId))
            {
#if UNITY_EDITOR
                DLog.Alert($"[FSM] CanExecute = false, dropping command {command.GetType().Name}",
                    EDlogColor.ORANGE
                    , AppConstants.show_log_unit_fsm);
#endif
                return;
            }

            TransitionTo(command.TargetState, command);
        }

        // ── TransitionTo ───────────────────────────────────────────────

        public void TransitionTo(UnitStateId targetId, IUnitCommand triggerCommand)
        {
            if (_alwaysForced.Contains(targetId))
            {
                ForceState(targetId, triggerCommand);
                return;
            }

#if UNITY_EDITOR
            if (_unit is SurvivorInstance)
            {
                
            }
#endif

            if (!_states.TryGetValue(targetId, out var next))
            {
                Debug.LogError($"[FSM] State {targetId} не зарегистрирован!");
                return;
            }

            if (next == _current) return;

            bool isInterrupt = triggerCommand is AbilityCommand;

            if (isInterrupt && _current != null)
                _stateStack.Push((_current, _currentCommand));

            _current?.OnExit(_unit);
            _current = next;
            _currentCommand = triggerCommand;
            _current.OnEnter(_unit, triggerCommand);

            OnStateChanged?.Invoke(CurrentStateId);
        }

        // ── PopState ───────────────────────────────────────────────────

        public void PopState()
        {
            if (_stateStack.Count == 0) return;

            var (prevState, prevCommand) = _stateStack.Pop();

            _current?.OnExit(_unit);
            _current = prevState;
            _currentCommand = prevCommand;
            _current.OnEnter(_unit, prevCommand);

            OnStateChanged?.Invoke(CurrentStateId); // ← новое
        }

        // ── ForceState ─────────────────────────────────────────────────

        public void ForceState(UnitStateId targetId) => ForceState(targetId, null);

        public void ForceState(UnitStateId targetId, IUnitCommand triggerCommand)
        {
            if (!_states.TryGetValue(targetId, out var next))
                return;

            if (_current?.StateId == UnitStateId.Dead)
                return;

            if (ShouldPushCurrent(targetId) && _current != null)
                _stateStack.Push((_current, _currentCommand));

            _current?.OnExit(_unit);
            _current = next;
            _currentCommand = triggerCommand;
            _current.OnEnter(_unit, triggerCommand);

            OnStateChanged?.Invoke(CurrentStateId);
        }

        private bool ShouldPushCurrent(UnitStateId targetId)
            => targetId == UnitStateId.Suppressed;
    }
}