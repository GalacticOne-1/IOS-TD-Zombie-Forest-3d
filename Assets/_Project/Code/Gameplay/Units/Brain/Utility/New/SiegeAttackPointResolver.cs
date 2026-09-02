using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Siege-specific выбор ближайшей валидной AttackPoint HQ.
    ///
    /// Не создаёт новую цель, не регистрирует ничего в TargetInfoRegistry —
    /// только вычисляет Vector3-позицию, к которой SiegeAdvanceAction ведёт
    /// зомби и от которой SiegeAttackHQAction меряет AttackRange.
    ///
    /// Стабильность (см. ТЗ п.15): пока текущая точка в blackboard валидна —
    /// используется она же, без пересчёта, чтобы избежать "дёргания" между
    /// think-тиками.
    /// </summary>
    internal static class SiegeAttackPointResolver
    {
        // ДОПУЩЕНИЕ: используется существующий A* Pathfinding Project API
        // (AstarPath.active.GetNearest), т.к. AIPath/Seeker уже применяются
        // в UnitMover. Если в проекте принят другой способ проверки
        // "точка на NavGraph и walkable" — заменить здесь единолично.
        private const float ReachableSnapRadius = 1.5f;
        private const float ReachableSnapRadiusSqr = ReachableSnapRadius * ReachableSnapRadius;

        public static Vector3 Resolve(
            UnitInstance unit,
            ITargetInfo headquarters,
            SiegeBlackboard blackboard)
        {
            var hqBase = headquarters as TargetInfoBase;
            var points = hqBase?.AttackPoints;

            // HQ без AttackPoints — старое поведение.
            if (points == null || points.Count == 0)
            {
                blackboard.CurrentAttackPoint = null;
                blackboard.ReacquireAttackPoint = false;
                return headquarters.Position;
            }

            // Используем текущую точку только если НЕ требуется
            // повторный выбор после смены objective.
            if (!blackboard.ReacquireAttackPoint &&
                IsStillValid(blackboard.CurrentAttackPoint, points))
            {
                return blackboard.CurrentAttackPoint.position;
            }

            Vector3 unitPos = unit.transform.position;

            Transform best = null;
            float bestSqDist = float.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];

                if (p == null || !p.gameObject.activeInHierarchy)
                    continue;

                if (!IsReachable(p.position))
                    continue;

                float sq = (p.position - unitPos).sqrMagnitude;

                if (sq < bestSqDist)
                {
                    bestSqDist = sq;
                    best = p;
                }
            }

            if (best == null)
            {
                blackboard.CurrentAttackPoint = null;
                blackboard.ReacquireAttackPoint = false;

                // Defensive fallback.
                return headquarters.Position;
            }

            blackboard.CurrentAttackPoint = best;

            // Точка выбрана заново.
            blackboard.ReacquireAttackPoint = false;

            return best.position;
        }

        private static bool IsStillValid(Transform point, IReadOnlyList<Transform> points)
        {
            if (point == null || !point.gameObject.activeInHierarchy) return false;

            bool stillMember = false;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == point)
                {
                    stillMember = true;
                    break;
                }
            }

            if (!stillMember) return false;

            return IsReachable(point.position);
        }

        private static bool IsReachable(Vector3 position)
        {
            if (AstarPath.active == null) return true; // defensive — нет графа, не блокируем AI

            var nn = AstarPath.active.GetNearest(position, NNConstraint.Default);
            if (nn.node == null || !nn.node.Walkable) return false;

            float sqDist = ((Vector3)nn.position - position).sqrMagnitude;
            return sqDist <= ReachableSnapRadiusSqr;
        }
    }
}