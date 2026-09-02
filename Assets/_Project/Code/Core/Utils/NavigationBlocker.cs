using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Единственное место, где строится GraphUpdateObject
    /// для блокировки/разблокировки клеток A* Grid Graph под зданиями.
    /// </summary>
    public static class NavigationBlocker
    {
        private const float BoundsExpand = 0.25f;

        /// <summary>
        /// Делает клетки под зданием непроходимыми.
        /// </summary>
        public static void Block(Bounds bounds, bool flush = false)
        {
            Apply(bounds, false, flush);
        }

        /// <summary>
        /// Возвращает клетки под зданием обратно в проходимое состояние.
        /// </summary>
        public static void Unblock(Bounds bounds, bool flush = false)
        {
            Apply(bounds, true, flush);
        }

        private static void Apply(Bounds bounds, bool walkable, bool flush)
        {
            if (AstarPath.active == null)
                return;

            bounds.Expand(BoundsExpand);

            var guo = new GraphUpdateObject(bounds)
            {
                modifyWalkability = true,
                setWalkability = walkable
            };

            AstarPath.active.UpdateGraphs(guo);

            if (flush)
                AstarPath.active.FlushGraphUpdates();
        }
    }
}