using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Raw sensor interface. No team logic, no hostility, no combat decisions.
    /// </summary>
    public interface IPerception
    {
        IReadOnlyList<ITargetInfo> GetVisibleTargets();
        bool HasVisibleTarget { get; }

        ITargetInfo GetNearestVisibleTarget();
        ITargetInfo GetTargetById(string targetId);

        /// <summary>
        /// Combined distance + LOS check.
        /// Use when you need both together (e.g. CanShoot validation).
        /// </summary>
        bool CanEngage(Vector3 origin, ITargetInfo target, float range);

        ITargetInfo FindNearestInRange(Vector3 origin, float range);

        /// <summary>
        /// Pure line-of-sight check. No distance gate.
        /// Use this when range is irrelevant or already validated separately.
        /// </summary>
        bool HasLineOfSight(Vector3 origin, Vector3 targetPos);
    }
}