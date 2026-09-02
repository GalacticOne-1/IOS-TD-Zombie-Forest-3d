using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Мозг игрового юнита.
    ///
    /// Изменения:
    ///   — Принимает PlayerBrainSettings через ctor вместо PerceptionConfig (SO).
    ///   — Больше нет обращений к ConfigProvider внутри Brain.
    ///   — Все параметры поведения берутся из runtime definition.
    /// </summary>
    public sealed class PlayerCommandBrain : IUnitBrain
    {
        private readonly PlayerBrainDefinition _definition;

        private UnitInstance _unit;
        private UnitStateMachine _fsm;
        private IPerception _perception;
        private SuppressionReceiver _suppression;
        private CoverFinder _coverFinder;
        private WeaponSlot _weaponSlot;

        public AICommandLock CommandLock { get; } = new();

        private CoverPoint _currentCover;
        private float _reEngageTimer;

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public PlayerCommandBrain(PlayerBrainDefinition definition)
        {
            _definition = definition ?? throw new System.ArgumentNullException(nameof(definition));
        }

        // ── IUnitBrain ────────────────────────────────────────────────────

        public void Initialize(UnitInstance unit)
        {
            _unit = unit;
            _fsm = unit.StateMachine;
            _perception = unit.PhysicsPerception;
            _suppression = unit.GetComponent<SuppressionReceiver>();
            _coverFinder = unit.GetComponent<CoverFinder>();
            _weaponSlot = unit.WeaponSlot;

            _suppression.OnSuppressed += HandleSuppressed;
            _suppression.OnSuppressionLifted += HandleSuppressionLifted;
        }

        public void Tick(float dt)
        {
            if (!_isEnabled) return;
            if (_reEngageTimer > 0f) _reEngageTimer -= dt;

            if (CheckPanic()) return;
            if (_suppression.IsSuppressed) return;
            if (CheckAutoReload()) return;
            if (CommandLock.AllowAutoAttack && CheckAutoEngage()) return;
            if (CommandLock.AllowAutoCover && CheckAutoCover()) return;
        }

        public void OnPlayerCommand(IUnitCommand command)
        {
            CommandLock.Lock(command);
            if (command is MoveCommand or FallBackCommand)
            {
                _currentCover?.Release(_unit as SurvivorInstance);
                _currentCover = null;
            }
        }

        public void OnStateChanged(UnitStateId newState)
        {
            if (newState == UnitStateId.Idle || newState == UnitStateId.SquadMoving)
                CommandLock.Unlock();
        }

        public void Dispose()
        {
            if (_suppression == null) return;
            _suppression.OnSuppressed -= HandleSuppressed;
            _suppression.OnSuppressionLifted -= HandleSuppressionLifted;
            _currentCover?.Release(_unit as SurvivorInstance);
        }

        // ── Priorities ────────────────────────────────────────────────────

        private bool CheckPanic() => false;

        private bool CheckAutoReload()
        {
            var weapon = _weaponSlot?.CurrentWeapon;
            if (weapon == null || weapon.State != WeaponState.Empty) return false;

            var weaponHandle = _unit.CurrentWeaponHandle;
            if (weaponHandle == null) return false;

            if (_unit.ReloadHandler.TryStartReload(weaponHandle.Entity))
            {
                _fsm.TransitionTo(UnitStateId.Idle, null);
                return true;
            }

            if (_fsm.CurrentStateId == UnitStateId.Engaging)
            {
                _fsm.ForceState(UnitStateId.Idle);
                return true;
            }

            float meleeRange = _unit.MeleeAttack?.AttackRange ?? 0f;
            if (meleeRange > 0f)
            {
                var nearest = TargetingUtility.FindNearestHostile(_unit);
                if (nearest != null)
                {
                    float dist = Vector3.Distance(_unit.transform.position, nearest.Position);
                    if (dist <= meleeRange)
                        _fsm.Execute(new AttackCommand(nearest.TargetId, UnitStateId.MeleeEngaging));
                }
            }

            return true;
        }

        private bool CheckAutoEngage()
        {
            if (_reEngageTimer > 0f) return false;

            // Параметры из PlayerBrainSettings (runtime definition), не SO
            var nearest = TargetingUtility.FindNearestHostileInRange(_unit, _definition.AutoEngageRange);
            if (nearest == null) return false;

            float dist = Vector3.Distance(_unit.transform.position, nearest.Position);
            bool hasWorkingWeapon = _weaponSlot?.CurrentWeapon != null
                                    && _weaponSlot.CurrentWeapon.State != WeaponState.Empty;
            float meleeRange = _unit.MeleeAttack?.AttackRange ?? 0f;
            bool inMeleeRange = meleeRange > 0f && dist <= meleeRange;

            if (!hasWorkingWeapon && !inMeleeRange) return false;

            var currentState = _fsm.CurrentStateId;
            if (currentState == UnitStateId.Engaging && hasWorkingWeapon) return true;
            if (currentState == UnitStateId.MeleeEngaging && !hasWorkingWeapon) return true;

            UnitStateId target = hasWorkingWeapon ? UnitStateId.Engaging : UnitStateId.MeleeEngaging;
            _fsm.Execute(new AttackCommand(nearest.TargetId, target));
            return true;
        }

        private bool CheckAutoCover()
        {
            if (_fsm.CurrentStateId == UnitStateId.TakingCover) return false;
            if (TargetingUtility.HasVisibleHostile(_unit)) return false;

            var nearest = TargetingUtility.FindNearestHostileInRange(_unit, _definition.AutoCoverRange);
            if (nearest == null) return false;

            var cover = _coverFinder.FindBest(
                _unit.transform.position, nearest.Position, _unit as SurvivorInstance);

            if (cover == null || !cover.TryOccupy(_unit as SurvivorInstance)) return false;

            if (_currentCover != null && _currentCover != cover)
                _currentCover.Release(_unit as SurvivorInstance);

            _currentCover = cover;
            _fsm.Execute(new TakeCoverCommand(cover.Position));
            return true;
        }

        private void HandleSuppressed()
        {
            _fsm.ForceState(UnitStateId.Suppressed);
            var nearest = TargetingUtility.FindNearestHostile(_unit);
            var threatPos = nearest?.Position
                            ?? _unit.transform.position + _unit.transform.forward * 5f;

            var cover = _coverFinder.FindBest(
                _unit.transform.position, threatPos, _unit as SurvivorInstance);

            if (cover != null && cover.TryOccupy(_unit as SurvivorInstance))
            {
                _currentCover?.Release(_unit as SurvivorInstance);
                _currentCover = cover;
            }
        }

        private void HandleSuppressionLifted()
        {
            if (_fsm.CurrentStateId != UnitStateId.Suppressed) return;

            bool hasEnemy = TargetingUtility.HasVisibleHostile(_unit);
            _fsm.ForceState(hasEnemy ? UnitStateId.Engaging : UnitStateId.Idle);
            if (hasEnemy) _reEngageTimer = _definition.ReEngageDelay;
        }

        public void ApplySuppression(Vector3 shotOrigin)
            => _suppression.ReceiveSuppression(shotOrigin);

        public void NotifyCommandCompleted()
            => CommandLock.Unlock();
    }
}