using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    // =========================================================
    // FormationFollower
    // =========================================================

    /// <summary>
    /// Конвертирует авторитетный Center + Forward в мировые позиции слотов.
    ///
    /// Пишет: slot.DesiredWorldPosition
    /// Читает: slot.LocalOffset (статика, не меняется во время движения)
    ///
    /// Не вызывает FormationSystem.GetOffset() — LocalOffset уже готов.
    /// Единственная операция: Quaternion.LookRotation + умножение на вектор.
    /// O(n) · дёшево.
    /// </summary>
    public sealed class FormationFollower
    {
        private readonly SquadFormationSlots _slots;

        public FormationFollower(SquadFormationSlots slots)
            => _slots = slots;

        public void Tick(Vector3 center, Vector3 formationHeading)
        {
            if (formationHeading == Vector3.zero)
                formationHeading = Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(formationHeading, Vector3.up);
            foreach (var slot in _slots.Slots)
                slot.DesiredWorldPosition = center + rotation * slot.LocalOffset;
        }
    }

    // =========================================================
    // SlotProjector
    // =========================================================

    /// <summary>
    /// Снапит желаемые позиции слотов на navmesh.
    ///
    /// Читает:  slot.DesiredWorldPosition
    /// Пишет:   slot.ProjectedWorldPosition
    ///          slot.IsProjected (только для дебага)
    ///
    /// Fallback стратегия: DesiredWorldPosition.
    /// Для открытых карт это оптимально — snap-ошибка минимальна,
    /// агент дойдёт до желаемой точки без навмеш-коррекции.
    /// LastValidWorldPosition намеренно не используется:
    /// может заморозить слот у края карты навсегда.
    /// </summary>
    public static class SlotProjector
    {
        private const float MaxSnapDistance = 2.5f;

        public static void Project(SquadSlot[] slots)
        {
            foreach (var slot in slots)
            {
                var nearest = AstarPath.active.GetNearest(slot.DesiredWorldPosition, NNConstraint.Walkable);

                bool valid = nearest.node != null
                             && nearest.node.Walkable
                             && Vector3.Distance(
                                 (Vector3)nearest.position,
                                 slot.DesiredWorldPosition) <= MaxSnapDistance;

                if (valid)
                {
                    slot.ProjectedWorldPosition = (Vector3)nearest.position;
                    slot.IsProjected = true;
                }
                else
                {
                    // Fallback: желаемая позиция без снапа.
                    // На открытой карте навмеш покрывает большинство точек —
                    // этот путь срабатывает только у границ карты.
                    slot.ProjectedWorldPosition = slot.DesiredWorldPosition;
                    slot.IsProjected = false;
                }
            }
        }
    }

    // =========================================================
    // SlotSeparator
    // =========================================================

    /// <summary>
    /// Однопроходное расталкивание перекрывающихся слотов.
    ///
    /// Читает:  slot.ProjectedWorldPosition
    /// Пишет:   slot.FinalWorldPosition
    ///
    /// O(n²) — для 6 агентов это 15 пар, на мобайле незаметно.
    /// Один проход без итерации — намеренно.
    /// Небольшое остаточное перекрытие допустимо и визуально незаметно.
    /// </summary>
    public static class SlotSeparator
    {
        private const float MinSeparation = 1.2f;

        public static void Separate(SquadSlot[] slots)
        {
            // Инициализируем FinalWorldPosition из Projected
            for (int i = 0; i < slots.Length; i++)
                slots[i].FinalWorldPosition = slots[i].ProjectedWorldPosition;

            // Один проход push-apart
            for (int i = 0; i < slots.Length; i++)
            for (int j = i + 1; j < slots.Length; j++)
            {
                Vector3 delta = slots[j].FinalWorldPosition - slots[i].FinalWorldPosition;
                float dist = delta.magnitude;

                if (dist >= MinSeparation || dist < 0.001f) continue;

                Vector3 push = delta.normalized * (MinSeparation - dist) * 0.5f;
                slots[i].FinalWorldPosition -= push;
                slots[j].FinalWorldPosition += push;
            }
        }
    }

    // =========================================================
    // SlotMovementDispatcher
    // =========================================================

    /// <summary>
    /// Кэширует последние выданные позиции и вызывает UnitMover.MoveTo()
    /// только при значимом изменении слота.
    ///
    /// Читает:  slot.FinalWorldPosition
    /// Вызывает: slot.Occupant.Mover.MoveTo() при delta > RepathDistance
    ///
    /// Без этого класса MoveTo() вызывался бы каждый кадр,
    /// спамя Seeker.StartPath() внутри UnitMover.
    /// </summary>
    public sealed class SlotMovementDispatcher
    {
        private const float RepathDistance = 0.5f;

        private readonly Vector3[] _lastIssued;

        public SlotMovementDispatcher(int slotCount)
        {
            _lastIssued = new Vector3[slotCount];
            Reset();
        }

        /// <summary>
        /// Сбрасывает кэш. Вызывается при IssueMove() чтобы первый тик
        /// гарантированно выдал команду всем агентам.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _lastIssued.Length; i++)
                _lastIssued[i] = Vector3.positiveInfinity;
        }

        public void Dispatch(SquadSlot[] slots, WorldInputDispatcher.MoveMode mode)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.Occupant == null) continue;

                float sqrDelta = (slot.FinalWorldPosition - _lastIssued[i]).sqrMagnitude;
                if (sqrDelta < RepathDistance * RepathDistance) continue;

                _lastIssued[i] = slot.FinalWorldPosition;

                // Обновляем DesiredPosition — SquadMovingState читает его в UpdateFormationDestination()
                slot.Occupant.DesiredPosition = slot.FinalWorldPosition;

                // Команда через StateMachine: переводит в SquadMovingState если ещё не там,
                // или вызывает HandleCommand если уже в нём
                slot.Occupant.StateMachine.Execute(new MoveCommand(slot.FinalWorldPosition, mode));
            }
        }
    }
}