using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units.Movement;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Zombie
{
    // ─────────────────────────────────────────────
    //  RoamingState
    //
    //  Зомби случайно бродит в радиусе roamRadius
    //  от своей стартовой позиции.
    //  Walk-анимация, скорость roamSpeed.
    //  При обнаружении цели Brain выдаст ChaseCommand → выход.
    // ─────────────────────────────────────────────

    public sealed class RoamingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Roaming;

        
        private readonly float roamSpeed;
        private readonly float roamRadius;
        
        private Vector3 _spawnPosition;
        private Vector3 _waypoint;
        private float _waypointTimer; // таймер ожидания на точке
        private const float WaitAtWaypointSec = 1.5f;

        
        
        public RoamingState(EnemyRuntimeDefinition definition)
        {
            roamRadius = definition.BrainDefinition.RoamRadius;
            roamSpeed = definition.MovementDefinition.WalkSpeed;
        }

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _spawnPosition = unit.EnemyAdapter.RuntimeBase.SpawnPosition;

            unit.Mover.SetSpeed(roamSpeed);

            PickNewWaypoint(unit);
        }

        public void OnExit(UnitInstance unit)
        {
        }

        public void Tick(UnitInstance unit, float dt)
        {
            if (_waypointTimer > 0f)
            {
                _waypointTimer -= dt;
                if (_waypointTimer <= 0f)
                    PickNewWaypoint(unit);
                return;
            }

            var navState = unit.Mover.State;

            // Arrived — ждём на точке
            if (navState == NavigationState.Arrived)
            {
                _waypointTimer = WaitAtWaypointSec;
                return;
            }

            // Failed (partial path) — сразу retry без ожидания
            if (navState == NavigationState.Failed)
            {
                _waypointTimer = WaitAtWaypointSec;
            }
        }

        public bool HandleCommand(UnitInstance unit, IUnitCommand command) => false;

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);

        // ─────────────────────────────────────────

        private void PickNewWaypoint(UnitInstance unit)
        {
            Vector2 offset = Random.insideUnitCircle * roamRadius;
            _waypoint = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
            unit.Mover.MoveTo(_waypoint, WorldInputDispatcher.MoveMode.Walk, false, result =>
            {
                // PathFailed приходит немедленно из OnPathComplete (path.error)
                // PartialPath/Success приходят через UpdateM при реальном arrival
                // Оба случая failure выставляют таймер — Tick разберётся через State
                if (result == NavigationMoveResult.PathFailed ||
                    result == NavigationMoveResult.InvalidDestination)
                    _waypointTimer = WaitAtWaypointSec;
            });
        }
    }

}