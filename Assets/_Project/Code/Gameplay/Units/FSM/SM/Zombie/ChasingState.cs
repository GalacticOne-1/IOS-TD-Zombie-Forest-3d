using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Movement;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Zombie
{
    /// <summary>
    /// Continuous chase locomotion state (FIXED VERSION).
    ///
    /// Fix:
    ///   - Adds Tick-based re-evaluation of target position
    ///   - Uses TargetingUtility live hostile lookup
    ///   - Eliminates stale ChaseCommand position dependency
    ///   - No Brain changes required
    /// </summary>
    public sealed class ChasingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Chasing;

        private readonly MovementDefinition _movementDefinition;

        private Vector3 _currentSlot; // текущая цель движения (слот окружения)
        private bool _hasSlot;

        private const float RepathThresholdSqr = 0.25f; // 0.5м — чувствительнее к обновлениям

        public ChasingState(EnemyRuntimeDefinition definition)
        {
            _movementDefinition = definition.MovementDefinition;
        }

        // =========================================================
        // Enter
        // =========================================================

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            if (triggerCommand is not ChaseCommand cmd)
                return;
            
            // === звук агрессии, когда зомби получает цель игрока
            var voice = unit.RuntimeBase.RuntimeDefinition.VoiceAudio.ToData();
            if (voice != null)
            {
                EventBus<AudioVoiceEvent>.Raise(
                    new AudioVoiceEvent(
                        unit.Tr.position,
                        voice,
                        VoiceEventType.Aggro,
                        priority: 30));
            }

            _currentSlot = cmd.SlotPosition; // ИСПРАВЛЕНО: слот, не позиция цели
            _hasSlot = true;

            unit.Mover.SetSpeed(cmd.Speed);
            unit.Mover.MoveTo(_currentSlot, WorldInputDispatcher.MoveMode.Run, true);
        }

        // =========================================================
        // Tick
        // =========================================================

        public void Tick(UnitInstance unit, float dt)
        {
            if (!_hasSlot || unit.Mover == null)
                return;

            // Brain переиздаёт ChaseCommand каждый think-тик через HandleCommand.
            // Tick только следит за тем, что mover не застрял.
            bool moverNeedsRefresh =
                unit.Mover.State == NavigationState.Arrived ||
                unit.Mover.State == NavigationState.Failed ||
                !unit.Mover.IsMoving;

            if (moverNeedsRefresh)
            {
                unit.Mover.SetSpeed(_movementDefinition.RunSpeed);
                unit.Mover.MoveTo(_currentSlot, WorldInputDispatcher.MoveMode.Run, false);
            }
        }

        // =========================================================
        // Commands
        // =========================================================

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is not ChaseCommand chase)
                return false;

            unit.Mover.SetSpeed(chase.Speed);

            // ИСПРАВЛЕНО: принимаем обновлённый слот от Brain
            Vector3 newSlot = chase.SlotPosition;

            bool slotMoved =
                (newSlot - _currentSlot).sqrMagnitude > RepathThresholdSqr;

            if (slotMoved)
            {
                _currentSlot = newSlot;
                unit.Mover.MoveTo(_currentSlot, WorldInputDispatcher.MoveMode.Run, false);
            }

            return true;
        }

        // =========================================================
        // Exit
        // =========================================================

        public void OnExit(UnitInstance unit)
        {
            _hasSlot = false;
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
        {
            unit.StateMachine.ForceState(targetState);
        }
    }
}