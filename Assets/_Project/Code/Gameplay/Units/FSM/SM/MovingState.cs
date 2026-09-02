using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  MovingState
    //
    //  Что делает:
    //    - Каждый Tick передаёт DesiredPosition в UnitMover
    //    - Когда прибыл (Mover.HasArrived) → переход в Idle
    //    - Параллельная авто-атака разрешена через CommandLock
    //
    //  Переходы ИЗ Moving:
    //    → Idle      : прибыли на место
    //    → Engaging  : AttackCommand (игрок или ReactiveAI)
    //    → Reloading : ReloadCommand
    //    → Suppressed: ForceState (SuppressionReceiver)
    //    → Dying     : ForceState (hp = 0)
    // ─────────────────────────────────────────────
    
    public sealed class MovingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.SquadMoving;
 
        private Vector3 _destination;
        private WorldInputDispatcher.MoveMode _moveMode;
 
        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            if (triggerCommand is MoveCommand move)
            {
                _destination = move.Destination;
                _moveMode    = move.MoveMode;
            }
            else if (triggerCommand is FallBackCommand fallback)
            {
                _destination = fallback.Destination;
                _moveMode    = WorldInputDispatcher.MoveMode.Run;
            }
            else
            {
                // AI-юниты могут войти без команды — берём текущую позицию как заглушку.
                // Конкретный мозг сразу выдаст MoveCommand в следующем Tick.
                _destination = unit.transform.position;
                _moveMode    = WorldInputDispatcher.MoveMode.Walk;
            }
 
            unit.Mover.MoveTo(_destination, _moveMode, false);
        }
 
        public void OnExit(UnitInstance unit) { }
 
        public void Tick(UnitInstance unit, float dt)
        {
            // DesiredPosition актуален только для игрока (ISquadMember).
            // Обновляем цель если юнит является членом отряда.
            if (unit is SurvivorInstance survivor)
            {
                if (Vector3.Distance(survivor.DesiredPosition, _destination) > 0.5f)
                {
                    _destination = survivor.DesiredPosition;
                    unit.Mover.MoveTo(_destination, _moveMode, false);
                }
            }
 
            if (unit.Mover.HasArrived)
                unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
        }
 
        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is MoveCommand newMove)
            {
                _destination = newMove.Destination;
                _moveMode    = newMove.MoveMode;
                unit.Mover.MoveTo(_destination, _moveMode, false);
                unit.Brain?.OnPlayerCommand(command);   // Brain — protected, доступен через свойство
                return true;
            }
 
            return false;
        }
 
        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);
    }
}