using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Units.Movement;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Zombie
{
    /// <summary>
    /// ИЗМЕНЕНИЯ ДЛЯ SIEGE: explicit AttackCommand.TargetId теперь authoritative,
    /// приведено к тому же принципу, что уже используется в EngagingState.
    /// Раньше Tick() всегда звал TargetingUtility.FindNearestHostile() напрямую,
    /// из-за чего AttackCommand(Wall)/AttackCommand(HQ) от Siege-actions
    /// игнорировались, если рядом оказывался игрок.
    /// </summary>
    public sealed class ZombieMeleeEngagingState : IUnitState
    {
        public UnitStateId StateId => UnitStateId.MeleeEngaging;

        private const float ExitPadding = 0.6f;
        private const float AttackRangePadding = 0.2f;
        private const float RepathInterval = 0.25f;
        private const float SlotRepathThreshSqr = 0.09f; // 0.3м
        private const float SlotReachedThresh = 0.35f;

        private readonly float _rotationSpeed;
        private readonly float _runSpeed;
        private readonly EnemyBlackboard _blackboard;

        private UnitInstance _unit;
        private MeleeAttackComponent _melee;
        private bool _isAttacking;
        private bool _pendingDisengage;
        private float _repathTimer;
        private Vector3 _lastSlotSentToMover;

        /// <summary>NEW — explicit target command, сохранённый при входе и
        /// обновляемый при ре-триггере от Brain.</summary>
        private IUnitCommand _enterCommand;

        public ZombieMeleeEngagingState(
            EnemyRuntimeDefinition definition,
            EnemyBlackboard blackboard)
        {
            _rotationSpeed = definition.MovementDefinition.RotationSpeed;
            _runSpeed = definition.MovementDefinition.RunSpeed;
            _blackboard = blackboard;
        }

        // ── Enter / Exit ──────────────────────────────────────────────────

        public void OnEnter(UnitInstance unit, IUnitCommand triggerCommand)
        {
            _unit = unit;
            _enterCommand = triggerCommand; // NEW
            _isAttacking = false;
            _pendingDisengage = false;
            _repathTimer = 0f;

            _melee = unit.MeleeAttack;
            if (_melee != null)
            {
                _melee.OnHitLogicComplete += OnHitLogicComplete;
                _melee.OnAttackAnimationRequested += OnAttackAnimationRequested;
            }

            unit.Mover.SetRotationControl(false);

            var targetId = (triggerCommand as AttackCommand)?.TargetId;
            var enterTarget = targetId != null
                ? TargetingUtility.FindHostileById(unit, targetId)
                : null;
            var enterEffectivePos = enterTarget != null
                ? GetEffectiveTargetPosition(enterTarget)
                : unit.transform.position;
            
            var slot = GetCurrentSlot(targetId, enterEffectivePos);
            _lastSlotSentToMover = slot;
            unit.Mover.SetSpeed(_runSpeed);
            unit.Mover.MoveTo(slot, WorldInputDispatcher.MoveMode.Run, true);
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
            _pendingDisengage = false;
            _melee = null;
            _unit = null;
            _enterCommand = null; // NEW
        }

        // ── Tick ──────────────────────────────────────────────────────────

        public void Tick(UnitInstance unit, float dt)
        {
            _repathTimer -= dt;

            var target = ResolveTarget(unit);
            if (target == null)
            {
                if (!_isAttacking)
                    unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
                return;
            }

            // NEW — для HQ используется attack point, а не сырой Position (центр здания).
            // Для обычных целей GetEffectiveTargetPosition возвращает target.Position
            // без изменений — поведение Raid не затронуто.
            Vector3 effectiveTargetPosition = GetEffectiveTargetPosition(target);

            RotateTowardsTarget(unit, effectiveTargetPosition);

            if (_melee == null)
            {
                unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
                return;
            }

            // CHANGED: было target.Position
            float distToTarget = Vector3.Distance(unit.transform.position, effectiveTargetPosition);
            float exitThreshold = _melee.AttackRange + ExitPadding;

            if (distToTarget > exitThreshold)
            {
                if (_isAttacking)
                {
                    _pendingDisengage = true;
                    return;
                }

                unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
                return;
            }

            // CHANGED: передаём target.TargetId
            Vector3 currentSlot = GetCurrentSlot(target.TargetId, effectiveTargetPosition);
            float distToSlot = Vector3.Distance(unit.transform.position, currentSlot);

            bool slotDrifted = (currentSlot - _lastSlotSentToMover).sqrMagnitude
                               > SlotRepathThreshSqr;
            bool notAtSlot = distToSlot > SlotReachedThresh;
            bool moverStuck = unit.Mover.State == NavigationState.Arrived
                              || unit.Mover.State == NavigationState.Failed
                              || !unit.Mover.IsMoving;

            if ((slotDrifted || (moverStuck && notAtSlot)) && _repathTimer <= 0f)
            {
                _lastSlotSentToMover = currentSlot;
                unit.Mover.SetSpeed(_runSpeed);
                unit.Mover.MoveTo(currentSlot, WorldInputDispatcher.MoveMode.Run, false);
                _repathTimer = RepathInterval;
            }

#if UNITY_EDITOR
            DLog.Alert(
                $"[Melee] {unit.name} " +
                $"target={target.TargetId} " +
                $"dist={distToTarget:F3} " +
                $"attackRange={_melee.AttackRange:F3} " +
                $"exit={exitThreshold:F3} " +
                $"distSlot={distToSlot:F3} " +
                $"slotReached={SlotReachedThresh:F3} " +
                $"moverState={unit.Mover.State} " +
                $"moving={unit.Mover.IsMoving}", AppConstants.show_log_unit_fsm);
#endif

            if (distToTarget > _melee.AttackRange + AttackRangePadding) return;

            if (notAtSlot == false && unit.Mover.IsMoving)
                unit.Mover.Stop();

            if (_isAttacking) return;
            if (!_melee.IsReady) return;

            _isAttacking = true;
            _melee.Execute();
        }

        // ── Commands ──────────────────────────────────────────────────────

        public bool HandleCommand(UnitInstance unit, IUnitCommand command)
        {
            if (command is AttackCommand atk)
            {
                _enterCommand = atk; // NEW — ре-триггер от Brain обновляет explicit target
                return true; // поглощаем, FSM не дёргается
            }

            return false;
        }

        public void ForceTransition(UnitInstance unit, UnitStateId targetState)
            => unit.StateMachine.ForceState(targetState);

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnHitLogicComplete()
        {
            _isAttacking = false;
            if (_pendingDisengage)
            {
                _pendingDisengage = false;
                _unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
            }
        }

        private void OnAttackAnimationRequested()
            => _unit?.AnimationController?.PlayMeleeAttack();

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// NEW — тот же принцип, что EngagingState.ResolveTarget(). Explicit
        /// TargetId остаётся authoritative, пока цель жива и валидна (проверки
        /// IsDead/TeamService — внутри FindHostileById). Fallback на
        /// FindNearestHostile ТОЛЬКО если explicit target пропал.
        /// </summary>
        private ITargetInfo ResolveTarget(UnitInstance unit)
        {
            if (_enterCommand is AttackCommand atk)
            {
                var forced = TargetingUtility.FindHostileById(unit, atk.TargetId);
                if (forced != null) return forced;
            }

            return TargetingUtility.FindNearestHostile(unit);
        }
        
        /// <summary>NEW — для HQ (multi-collider target) возвращает выбранную
        /// SiegeAttackPointResolver-ом позицию вместо сырого target.Position
        /// (центра здания). Для обычных Raid-целей (обычный EnemyBlackboard,
        /// не SiegeBlackboard) поведение НЕ меняется.</summary>
        // private Vector3 GetEffectiveTargetPosition(ITargetInfo target)
        // {
        //     if (_blackboard is SiegeBlackboard siegeBb
        //         && siegeBb.Headquarters != null
        //         && target.TargetId == siegeBb.Headquarters.TargetId
        //         && siegeBb.CurrentAttackPoint != null)
        //     {
        //         return siegeBb.CurrentAttackPoint.position;
        //     }
        //
        //     return target.Position;
        // }
        private Vector3 GetEffectiveTargetPosition(ITargetInfo target)
        {
            if (target == null)
                return _unit != null
                    ? _unit.transform.position
                    : Vector3.zero;

            // ── Siege HQ ─────────────────────────────────────────────────────
            // HQ уже имеет собственную систему AttackPoint.
            // Она имеет приоритет над Collider.ClosestPoint().
            if (_blackboard is SiegeBlackboard siegeBb
                && siegeBb.Headquarters != null
                && target.TargetId == siegeBb.Headquarters.TargetId
                && siegeBb.CurrentAttackPoint != null)
            {
                return siegeBb.CurrentAttackPoint.position;
            }

            // ── Siege facilities (walls, buildings) ─────────────────────────
            // Для осады используем ближайшую физическую точку коллайдера,
            // а не transform.position (центр объекта).
            if (_blackboard is SiegeBlackboard
                && target is FacilityTargetInfo)
            {
                return target.GetClosestPoint(_unit.transform.position);
            }

            // ── Normal Raid targets ──────────────────────────────────────────
            // Полностью сохраняем старое поведение.
            return target.Position;
        }

        /// <summary>NEW — для HQ (multi-collider target) возвращает выбранную
        /// SiegeAttackPointResolver-ом позицию. Для остальных Siege-целей (стены)
        /// pack slot никогда не резервируется — SiegeAttackWallAction/SiegeAttackHQAction
        /// не используют PackReservation, это facility-цели, а не "окружаемые" юниты.
        /// Раньше отсутствие pack slot приводило к fallback на _lastSlotSentToMover,
        /// который на первом входе в состояние равен Vector3.zero (не инициализирован) —
        /// юнит строил путь к мировому нулю вместо стены. Теперь для любой Siege-цели
        /// без pack slot используется effectiveTargetPosition (уже вычисленная в Tick/OnEnter).
        /// Для обычных Raid-целей (EnemyBlackboard, не SiegeBlackboard) поведение
        /// не меняется — там pack slot всегда актуален для чейз-сценариев.</summary>
        private Vector3 GetCurrentSlot(string targetId, Vector3 effectiveTargetPosition)
        {
            if (_blackboard is SiegeBlackboard siegeBb)
            {
                if (siegeBb.Headquarters != null
                    && targetId == siegeBb.Headquarters.TargetId
                    && siegeBb.CurrentAttackPoint != null)
                {
                    return siegeBb.CurrentAttackPoint.position;
                }

                // NEW: любая другая Siege-цель без attack point (стена) — идём прямо
                // к цели, а не к несуществующему pack slot.
                if (!_blackboard.HasPackSlot)
                    return effectiveTargetPosition;
            }

            if (_blackboard == null || !_blackboard.HasPackSlot)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[ZombieMeleeEngagingState] {_unit?.name}: нет pack слота, " +
                    "используем последнюю известную позицию.");
#endif
                return _lastSlotSentToMover;
            }

            return _blackboard.PackSlotPosition;
        }

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
    }
}
