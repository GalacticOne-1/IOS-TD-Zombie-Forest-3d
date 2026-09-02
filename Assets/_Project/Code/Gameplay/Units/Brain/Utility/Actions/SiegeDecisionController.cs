using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// ЕДИНСТВЕННОЕ место в Siege AI, которое вызывает unit.StateMachine.Execute().
    ///
    /// SiegeObjectiveResolver уже определяет CurrentObjective (Player/Wall/HQ) —
    /// этот класс не трогает objective, только конвертирует его в РОВНО ОДНУ
    /// команду за Tick(). Actions/скоринг для Siege больше не используются —
    /// решение Chase/Attack внутри группы принимается детерминированно по
    /// дистанции, а не через utility score.
    ///
    /// Критический момент: ChaseCommand.CanExecute() отклоняет команду из
    /// состояния MeleeEngaging (см. ChaseCommand). Значит при смене цели
    /// во время анимации атаки (например Wall -> Player) простой
    /// StateMachine.Execute(newCommand) был бы молча отброшен FSM, и юнит
    /// завис бы в бою со старой целью. EnsureClean() принудительно выводит
    /// юнита в Idle (снимая animation lock через ZombieMeleeEngagingState.OnExit)
    /// ПЕРЕД выдачей новой команды — но только когда цель реально сменилась.
    /// </summary>
    public sealed class SiegeDecisionController
    {
        private readonly float _attackRange;
        private readonly float _moveSpeed;
        private readonly EnemyAIDefinition _brainDef;

        public SiegeDecisionController(float attackRange, float moveSpeed, EnemyAIDefinition brainDef)
        {
            _attackRange = attackRange;
            _moveSpeed = moveSpeed;
            _brainDef = brainDef;
        }

        public void Tick(UnitInstance unit, SiegeAIContext ctx, SiegeBlackboard bb)
        {
            switch (bb.CurrentObjective)
            {
                case SiegeObjective.Player:
                    TickPlayer(unit, ctx, bb);
                    break;
                case SiegeObjective.Wall:
                    TickWall(unit, ctx, bb);
                    break;
                case SiegeObjective.Headquarters:
                    TickHeadquarters(unit, ctx, bb);
                    break;
            }
        }

        // ── Player ───────────────────────────────────────────────────────

        private void TickPlayer(UnitInstance unit, SiegeAIContext ctx, SiegeBlackboard bb)
        {
            if (!ctx.HasVisibleTarget) return; // резолвер не должен был выбрать Player без цели, но defensive

            string targetId = ctx.VisibleTarget.TargetId;

            if (ctx.DistanceToVisibleTarget <= _attackRange && IsEnabled(AIActionType.Attack))
            {
                IssueAttack(unit, bb, targetId);
                return;
            }

            if (!IsEnabled(AIActionType.Chase)) return;

            Vector3 slotPos = bb.PackReservation.EnsureSlot(
                targetId, ctx.VisibleTargetPosition, unit, bb);
            IssueChase(unit, bb, targetId, slotPos);
        }

        // ── Wall ─────────────────────────────────────────────────────────

        private void TickWall(UnitInstance unit, SiegeAIContext ctx, SiegeBlackboard bb)
        {
            var wall = ctx.BlockingWall;
            if (wall == null || wall.IsDead) return;

            string targetId = wall.TargetId;
            Vector3 attackPos = wall.GetClosestPoint(unit.transform.position);
            float dist = Vector3.Distance(unit.transform.position, attackPos);

            if (dist <= _attackRange && IsEnabled(AIActionType.AttackWall))
            {
                IssueAttack(unit, bb, targetId);
                return;
            }

            if (!IsEnabled(AIActionType.Chase)) return;
            IssueChase(unit, bb, targetId, attackPos);
        }

        // ── Headquarters ─────────────────────────────────────────────────

        private void TickHeadquarters(UnitInstance unit, SiegeAIContext ctx, SiegeBlackboard bb)
        {
            if (ctx.Headquarters == null || ctx.Headquarters.IsDead) return;

            string targetId = ctx.Headquarters.TargetId;
            float dist = Vector3.Distance(unit.transform.position, ctx.HeadquartersAttackPosition);

            if (dist <= _attackRange && IsEnabled(AIActionType.AttackHQ))
            {
                IssueAttack(unit, bb, targetId);
                return;
            }

            if (!IsEnabled(AIActionType.AdvanceToHQ)) return;
            // CHANGED: раньше ChaseCommand к HQ использовал магическую строку
            // "_hq" вместо реального TargetId — из-за этого EnsureClean не мог
            // отличить "chase HQ" от "attack HQ" как одной и той же цели.
            // Теперь везде используется ctx.Headquarters.TargetId.
            IssueChase(unit, bb, targetId, ctx.HeadquartersAttackPosition);
        }

        // ── Command issuance ─────────────────────────────────────────────

        private void IssueAttack(UnitInstance unit, SiegeBlackboard bb, string targetId)
        {
            EnsureClean(unit, bb, targetId);
            bb.ActiveCommandTargetId = targetId;
            bb.LastChosenState = UnitStateId.MeleeEngaging;
            bb.CommitTimeRemaining = 0.3f;
            unit.StateMachine.Execute(new AttackCommand(targetId, UnitStateId.MeleeEngaging));
        }

        private void IssueChase(UnitInstance unit, SiegeBlackboard bb, string targetId, Vector3 destination)
        {
            EnsureClean(unit, bb, targetId);
            bb.ActiveCommandTargetId = targetId;
            bb.LastChosenState = UnitStateId.Chasing;
            bb.CommitTimeRemaining = 0.3f;
            unit.StateMachine.Execute(new ChaseCommand(targetId, destination, _moveSpeed));
        }

        /// <summary>
        /// Форсированный выход из MeleeEngaging ТОЛЬКО если цель реально сменилась
        /// (не на каждый тик — иначе анимация атаки никогда бы не завершалась).
        /// TransitionTo(Idle) вызывает ZombieMeleeEngagingState.OnExit():
        /// Stop() mover, SetRotationControl(true), AnimationController.CombatExit(),
        /// отписка от melee-событий — это и есть "остановить атаку +
        /// очистить animation lock" из требований.
        /// </summary>
        private void EnsureClean(UnitInstance unit, SiegeBlackboard bb, string newTargetId)
        {
            if (bb.ActiveCommandTargetId == newTargetId) return;
            if (unit.StateMachine.CurrentStateId != UnitStateId.MeleeEngaging) return;

            unit.StateMachine.TransitionTo(UnitStateId.Idle, null);
        }

        private bool IsEnabled(AIActionType type)
            => !_brainDef.TryGetAction(type, out var def) || def.Enabled;
    }
}