using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units.Movement;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.States.Survivor
{
    /// <summary>
    /// Formation-aware movement state for squad-controlled survivors.
    ///
    /// Responsibilities:
    /// - Follow DesiredPosition assigned by SquadMovementSystem
    /// - Repath only when formation slot changes significantly
    /// - Transition to Idle when movement completes
    ///
    /// Does NOT:
    /// - Compute formation
    /// - Validate formation positions
    /// - Perform tactical reasoning
    /// </summary>
    public sealed class SquadMovingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.SquadMoving;

        // =========================================================
        // Config
        // =========================================================

        private const float RepathDistance = 0.5f;
        private const float RepathInterval = 0.2f;

        // =========================================================
        // Runtime
        // =========================================================

        private SurvivorInstance _unit;

        private Vector3 _destination;

        private WorldInputDispatcher.MoveMode _moveMode;

        private float _repathTimer;

        // =========================================================
        // Enter
        // =========================================================

        public void OnEnter(
            UnitInstance unit,
            IUnitCommand triggerCommand)
        {
            _unit = unit as SurvivorInstance;

            if (_unit == null)
            {
                Debug.LogError(
                    "[SquadMovingState] Requires SurvivorInstance");

                return;
            }

            ResolveInitialDestination(triggerCommand);

            RequestMove();
        }

        // =========================================================
        // Exit
        // =========================================================

        public void OnExit(UnitInstance unit)
        {
            _unit = null;
        }

        // =========================================================
        // Tick
        // =========================================================

        public void Tick(UnitInstance unit, float dt)
        {
            _repathTimer -= dt;

            UpdateFormationDestination();

            // if (_unit.Mover.HasArrived)
            // {
            //     _unit.StateMachine
            //         .TransitionTo(UnitStateId.Idle, null);
            // }
            
            // Юнит сам выходит из движения, как только перестал физически двигаться —
            // не дожидаясь, пока весь отряд встанет на свои слоты.
            // IsMoving (а не HasArrived) — по той же причине, что и в
            // SquadMovementSystem.AreAgentsAtFinalSlots: юнит должен выйти из
            // Moving, даже если не смог дойти до финальной точки (упёрся в препятствие).
            if (!_unit.Mover.IsMoving)
            {
                _unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
            }
        }

        // =========================================================
        // Commands
        // =========================================================

        public bool HandleCommand(
            UnitInstance unit,
            IUnitCommand command)
        {
            if (command is not MoveCommand move)
                return false;

            _destination = move.Destination;
            _moveMode = move.MoveMode;

            RequestMove();

            _unit.Brain?.OnPlayerCommand(command);

            return true;
        }

        // =========================================================
        // Force
        // =========================================================

        public void ForceTransition(
            UnitInstance unit,
            UnitStateId targetState)
        {
            unit.StateMachine.ForceState(targetState);
        }

        // =========================================================
        // Private
        // =========================================================

        private void ResolveInitialDestination(IUnitCommand triggerCommand)
        {
            if (triggerCommand is MoveCommand move)
            {
                _destination = move.Destination;
                _moveMode = move.MoveMode;
                return;
            }

            if (triggerCommand is FallBackCommand fallback)
            {
                _destination = fallback.Destination;
                _moveMode = WorldInputDispatcher.MoveMode.Run;
                return;
            }

            _destination = _unit.DesiredPosition;
            _moveMode = WorldInputDispatcher.MoveMode.Walk;
        }

        private void UpdateFormationDestination()
        {
            if (_repathTimer > 0f)
                return;

            Vector3 desired = _unit.DesiredPosition;

            float sqrDist = (desired - _destination).sqrMagnitude;

            if (sqrDist < RepathDistance * RepathDistance)
                return;

            _destination = desired;

            RequestMove();

            _repathTimer = RepathInterval;
        }

        private void RequestMove()
        {
            _unit.Mover.MoveTo(
                _destination,
                _moveMode,
                false,
                OnMoveResult);
        }

        private void OnMoveResult(
            NavigationMoveResult result)
        {
            switch (result)
            {
                case NavigationMoveResult.Success:
                    return;

                case NavigationMoveResult.PathFailed:
                case NavigationMoveResult.InvalidDestination:

                    // Пока fallback простой:
                    // просто остаёмся в текущем состоянии.
                    // SquadMovementSystem позже
                    // обновит DesiredPosition.

                    break;
            }
        }
    }
}