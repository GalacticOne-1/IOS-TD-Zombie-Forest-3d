using Galactic1.Game.Meta.Items;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// Проверка валидности таргета (range, navmesh, etc)
    /// </summary>
    public sealed class TargetingValidationService
    {
        
        // старая проверка для броска гранаты не годится, т.к проверяет навмеш
        // но для турели бодет ок
        public bool Validate(
            Vector3 origin,
            Vector3 target,
            UseModule config,
            out Vector3 projected)
        {
            projected = target;

            var start = AstarPath.active.GetNearest(origin);
            var end   = AstarPath.active.GetNearest(target);

            if (end.node == null || !end.node.Walkable)
                return false;

            if (!PathUtilities.IsPathPossible(start.node, end.node))
                return false;

            projected = end.clampedPosition;

            float range = config.Range;

            if (Vector3.Distance(origin, projected) > range)
                return false;

            return true;
        }
    }

}