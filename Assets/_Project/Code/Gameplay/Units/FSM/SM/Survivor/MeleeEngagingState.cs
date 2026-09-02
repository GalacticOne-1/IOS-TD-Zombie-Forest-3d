using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  MeleeEngagingState  (player-oriented, command-driven)
    //
    //  Target acquisition: TargetingUtility (single pipeline).
    //  Combat validation:  ICombatLogic.
    //  Does NOT call TeamService directly.
    // ─────────────────────────────────────────────

    public sealed class MeleeEngagingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.MeleeEngaging;

        private bool _isAttacking;
        private bool _wasMoving;
        private IUnitCommand _pendingCommand;
        private UnitStateId? _pendingTransition;
        private UnitInstance _unit;
        private IUnitCommand _enterCommand;
        private MeleeAttackComponent _melee;
        private float _moveUpdateTimer;
        private readonly float _rotationSpeed;

        public MeleeEngagingState(float rotationSpeed)
        {
            _rotationSpeed = rotationSpeed;
        }

        private static int GetCommandPriority(IUnitCommand command)
        {
            return command switch
            {
                TakeCoverCommand   => 100,
                FallBackCommand    => 90,
                MoveCommand        => 80,
                EquipWeaponCommand => 70,
                AttackCommand      => 10,
                _                  => 0
            };
        }

        // ── Enter / Exit ──────────────────────────────────────────────────

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _unit = unit;
            _enterCommand = triggerCommand;
            _isAttacking = false;
            _wasMoving = unit.Mover.IsMoving;
            _pendingCommand = null;
            _pendingTransition = null;
            _moveUpdateTimer = 0f;

            _melee = unit.MeleeAttack;
            if (_melee != null)
            {
                _melee.OnHitLogicComplete += OnHitLogicComplete;
                _melee.OnAttackAnimationRequested += OnAttackAnimationRequested;
                _melee.Reset();
            }

            unit.Mover.SetRotationControl(false);
        }

        public void OnExit(UnitInstance unit)
        {
            if (_melee != null)
            {
                _melee.OnHitLogicComplete -= OnHitLogicComplete;
                _melee.OnAttackAnimationRequested -= OnAttackAnimationRequested;
            }

            unit.Mover.Stop();
            unit.Mover.SetRotationControl(true);
            unit.AnimationController?.CombatExit();

            _isAttacking = false;
            _pendingCommand = null;
            _pendingTransition = null;
            _melee = null;
        }

        // ── Tick ──────────────────────────────────────────────────────────

        public void Tick(UnitInstance unit, float dt)
        {
            var target = ResolveTarget(unit);

            if (target != null)
                RotateTowardsTarget(unit, target.Position);

            if (target == null)
            {
                if (!_isAttacking)
                    TransitionTo(unit, _wasMoving ? UnitStateId.SquadMoving : UnitStateId.Idle);
                else
                    _pendingTransition = UnitStateId.Idle;
                return;
            }

            if (_melee == null)
            {
                TransitionTo(unit, UnitStateId.Idle);
                return;
            }

            float dist = Vector3.Distance(unit.transform.position, target.Position);

            if (dist > _melee.AttackRange)
                return; // Brain/AI manages closing via MoveTo

            if (_isAttacking)
                return;

            if (unit.Mover.IsMoving)
                unit.Mover.Stop();

            if (!_melee.IsReady)
                return;

            _isAttacking = true;
            _melee.Execute();
        }

        // ── HandleCommand ─────────────────────────────────────────────────

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is EquipWeaponCommand)
            {
                _isAttacking = false;
                return false;
            }

            if (_isAttacking)
            {
                if (command is AttackCommand)
                    return true;

                if (command is MoveCommand
                    or TakeCoverCommand
                    or FallBackCommand
                    or EquipWeaponCommand)
                {
                    SetPendingCommand(command);
                    return true;
                }
            }

            return false;
        }

        private void SetPendingCommand(IUnitCommand command)
        {
            if (command == null) return;

            if (_pendingCommand == null)
            {
                _pendingCommand = command;
                return;
            }

            if (GetCommandPriority(command) >= GetCommandPriority(_pendingCommand))
                _pendingCommand = command;
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnHitLogicComplete()
        {
            _isAttacking = false;
            if (_unit == null) return;

            if (_pendingTransition.HasValue)
            {
                var state = _pendingTransition.Value;
                _pendingTransition = null;

                if (_pendingCommand != null)
                {
                    var cmd = _pendingCommand;
                    _pendingCommand = null;
                    TransitionTo(_unit, state);
                    _unit.StateMachine.Execute(cmd);
                    return;
                }

                TransitionTo(_unit, state);
                return;
            }

            if (_pendingCommand != null)
            {
#if UNITY_EDITOR
                DLog.Alert($"[MeleeEngaging] Execute pending: {_pendingCommand?.GetType().Name}", EDlogColor.YELLOW);
#endif
                var cmd = _pendingCommand;
                _pendingCommand = null;
                _unit.StateMachine.Execute(cmd);
            }
        }

        private void OnAttackAnimationRequested()
            => _unit?.AnimationController?.PlayMeleeAttack();

        // ── Helpers ───────────────────────────────────────────────────────

        private void RotateTowardsTarget(UnitInstance unit, Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - unit.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            unit.transform.rotation = Quaternion.RotateTowards(
                unit.transform.rotation,
                Quaternion.LookRotation(dir),
                _rotationSpeed * Time.deltaTime);
        }

        private ITargetInfo ResolveTarget(UnitInstance unit)
        {
            // Explicit player-commanded target takes priority
            if (_enterCommand is AttackCommand atk)
            {
                // TargetingUtility.FindHostileById validates alive + visible + hostile
                var forced = TargetingUtility.FindHostileById(unit, atk.TargetId);
                if (forced != null) return forced;
            }

            // Fallback: nearest hostile via unified targeting pipeline
            return TargetingUtility.FindNearestHostile(unit);
        }

        private void TransitionTo(UnitInstance unit, UnitStateId state)
            => unit.StateMachine.TransitionTo(state, null);
    }
}