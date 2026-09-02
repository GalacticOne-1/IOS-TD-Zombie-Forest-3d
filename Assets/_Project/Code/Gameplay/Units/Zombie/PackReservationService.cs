using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Zombie
{
    /// <summary>
    /// Инкапсулирует жизненный цикл pack-слота для одного юнита.
    ///
    /// Решает проблему #2: Brain знал про PackCoordinator и управлял
    /// слотами напрямую — это нарушение SRP.
    ///
    /// Теперь:
    ///   Brain — оркестрирует think-loop, не знает про pack.
    ///   PackChaseAction — использует PackReservationService для Evaluate/Execute.
    ///   PackReservationService — единственное место управления слотами.
    ///
    /// Вызов Dispose() освобождает слот — Brain делает это через
    /// IPackReservation.Dispose(), не зная деталей реализации.
    /// </summary>
    public sealed class PackReservationService
    {
        private readonly PackCoordinator _coordinator;
        private readonly EnemyPackDefinition _pack;

        // Текущий зарезервированный targetId
        private string _reservedTargetId;

        public bool HasSlot => _reservedTargetId != null;

        public PackReservationService(PackCoordinator coordinator, EnemyPackDefinition pack)
        {
            _coordinator = coordinator;
            _pack = pack;
        }

        // ── PURE — вызывается из Evaluate() ──────────────────────────────

        /// <summary>
        /// Возвращает лучшую позицию слота без резервирования.
        /// PURE: не мутирует состояние координатора.
        /// </summary>
        public Vector3 PeekSlotPosition(
            string targetId,
            Vector3 targetPosition,
            UnitInstance unit)
        {
            return _coordinator.PeekBestSlot(targetId, targetPosition, unit, _pack);
        }

        // ── MUTATION — вызывается из Execute() ───────────────────────────

        /// <summary>
        /// Занимает слот и обновляет внутренний targetId.
        /// Если цель сменилась — освобождает старый слот автоматически.
        /// </summary>
        public Vector3 EnsureSlot(
            string targetId,
            Vector3 targetPosition,
            UnitInstance unit,
            EnemyBlackboard blackboard)
        {
            if (_reservedTargetId != targetId)
            {
                Release(unit);
                _coordinator.ReserveSlot(targetId, targetPosition, unit, _pack);
                _reservedTargetId = targetId;
                blackboard.ReservedTargetId = targetId;
            }

            // Возвращаем актуальную позицию занятого слота
            Vector3 pos = _coordinator.GetReservedSlotPosition(
                targetId, targetPosition, unit, _pack);

            blackboard.PackSlotPosition = pos;
            //DLog.Alert($"{unit.name} slot={pos}", EDlogColor.ORANGE);
            return pos;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────

        /// <summary>
        /// Освобождает текущий слот. Вызывается при смерти или смене цели.
        /// Brain вызывает это через Dispose() — без знания деталей.
        /// </summary>
        public void Release(UnitInstance unit)
        {
            if (_reservedTargetId == null) return;
            _coordinator.ReleaseSlot(_reservedTargetId, unit);
            _reservedTargetId = null;
        }
    }
}