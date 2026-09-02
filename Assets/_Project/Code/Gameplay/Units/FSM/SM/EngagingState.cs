using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  EngagingState  (player ranged combat)
    //
    //  Target acquisition: TargetingUtility (single pipeline).
    //  Combat validation:  ICombatLogic.CanAttack with weapon range.
    //  No direct TeamService or FindBestTarget calls.
    // ─────────────────────────────────────────────

    public sealed class EngagingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.Engaging;

        private bool _isFiring;
        private bool _wasMoving;
        private IUnitCommand _pendingCommand;
        private UnitStateId? _pendingTransition;
        private UnitInstance _unit;
        private IUnitCommand _enterCommand;
        private IWeaponWithEvents _subscribedWeapon;
        private float _fireRequestCooldown;

        private const float ReadyToFireAngle = 3f;

        // ── Enter / Exit ──────────────────────────────────────────────────

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _unit = unit;
            _enterCommand = triggerCommand;
            _isFiring = false;
            _wasMoving = unit.Mover.IsMoving;
            _pendingCommand = null;
            _pendingTransition = null;

            _subscribedWeapon = unit.WeaponSlot?.CurrentWeapon;
            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.OnShotLogicComplete += OnShotLogicComplete;
                _subscribedWeapon.OnStateChanged += OnWeaponStateChanged;
            }

            unit.Mover.SetRotationControl(false);

            DLog.Alert($"[Engaging] OnEnter | weapon={_subscribedWeapon} | cmd={triggerCommand?.GetType().Name}",
                EDlogColor.BLUE,
                AppConstants.show_log_unit_fsm);
        }

        public void OnExit(UnitInstance unit)
        {
            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.OnShotLogicComplete -= OnShotLogicComplete;
                _subscribedWeapon.OnStateChanged -= OnWeaponStateChanged;
            }

            unit.WeaponSlot?.AnimBridge?.StopFiring();

            _isFiring = false;
            _pendingCommand = null;
            _pendingTransition = null;

            unit.Mover.SetRotationControl(true);
        }

        // ── Tick ──────────────────────────────────────────────────────────

        public void Tick(UnitInstance unit, float dt)
        {
            var target = ResolveTarget(unit);

            if (target != null)
                RotateTowardsTarget(unit, target.AimPoint);

            if (_fireRequestCooldown > 0f)
            {
                _fireRequestCooldown -= dt;
                return;
            }

            var weapon = unit.WeaponSlot?.CurrentWeapon;
            if (weapon == null)
            {
                TransitionTo(unit, _wasMoving ? UnitStateId.SquadMoving : UnitStateId.Idle);
                return;
            }

            if (target == null)
            {
                if (!_isFiring) TransitionTo(unit, _wasMoving ? UnitStateId.SquadMoving : UnitStateId.Idle);
                else _pendingTransition = UnitStateId.Idle;
                return;
            }

            // Validate with weapon range — CombatLogic no longer reads WeaponSlot itself
            float weaponRange = weapon.Definition.Range;
            //if (!unit.CombatLogic.CanAttack(unit, target, weaponRange)) return;
            if (!unit.CombatLogic.CanAttack(unit, target, weaponRange))
            {
                TransitionTo(unit, UnitStateId.Idle);
                return;
            }

            if (!_isFiring && weapon.CanFire)
            {
                if (!IsAimedAtTarget(unit, target.AimPoint)) return;

                _isFiring = true;
                weapon.Fire(new FireContext(Vector3.Distance(unit.transform.position, target.Position), target.AimPoint));

                if (weapon.Definition.FireMode == FireMode.FullAuto)
                    _fireRequestCooldown = 60f / weapon.Definition.RoundsPerMinute;
            }
            else if (!_isFiring && !weapon.CanFire && weapon.State == WeaponState.Empty)
            {
                if (unit.ReloadHandler != null &&
                    unit.CurrentWeaponHandle != null &&
                    unit.ReloadHandler.TryStartReload(unit.CurrentWeaponHandle.Entity))
                {
                    unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
                }
                else
                {
                    float meleeRange = unit.MeleeAttack?.AttackRange ?? 0f;
                    if (meleeRange > 0f)
                    {
                        float dist = Vector3.Distance(unit.transform.position, target.Position);
                        if (dist <= meleeRange)
                            unit.StateMachine.Execute(
                                new AttackCommand(target.TargetId, UnitStateId.MeleeEngaging));
                    }
                }
            }
        }

        // ── HandleCommand ─────────────────────────────────────────────────

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is EquipWeaponCommand)
            {
                _isFiring = false;
                return false;
            }

            if (command is AttackCommand atk && atk.TargetState == UnitStateId.MeleeEngaging)
            {
                _isFiring = false;
                return false;
            }

            if (_isFiring &&
                command is MoveCommand or TakeCoverCommand or AttackCommand or FallBackCommand)
            {
                _pendingCommand = command;
                return true;
            }

            return false;
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnShotLogicComplete()
        {
            _isFiring = false;
            if (_unit == null) return;

            if (_pendingTransition.HasValue)
            {
                TransitionTo(_unit, _pendingTransition.Value);
                return;
            }

            if (_pendingCommand != null)
            {
                var cmd = _pendingCommand;
                _pendingCommand = null;
                _unit.StateMachine.Execute(cmd);
            }
        }

        private void OnWeaponStateChanged(WeaponState state)
        {
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void RotateTowardsTarget(UnitInstance unit, Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - unit.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            unit.transform.rotation = Quaternion.RotateTowards(
                unit.transform.rotation,
                Quaternion.LookRotation(dir),
                600f * Time.deltaTime);
        }

        private bool IsAimedAtTarget(UnitInstance unit, Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - unit.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return true;
            return Vector3.Angle(unit.transform.forward, dir) <= ReadyToFireAngle;
        }

        private ITargetInfo ResolveTarget(UnitInstance unit)
        {
            // Explicit player-commanded target takes priority
            if (_enterCommand is AttackCommand atk)
            {
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