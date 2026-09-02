
using Galactic1.AbstractFactory;

namespace Galactic1.Code.Gameplay.Damage
{
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;

    public static class TargetFinderService
    {
        public static Transform FindTarget(
            Vector3 origin,
            float radius,
            LayerMask targetLayer,
            TargetSelectionMode mode,
            TargetPriorityProfile profile = null // 👈 новый параметр
        )
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, targetLayer);
            if (hits.Length == 0)
                return null;

            List<Collider2D> targets = hits.ToList();
            
            if (targets == null || targets.Count == 0) return null;

            switch (mode)
            {
                case TargetSelectionMode.Closest:
                    return targets
                        .OrderBy(h => Vector3.Distance(origin, h.transform.position))
                        .First().transform;

                case TargetSelectionMode.Weakest:
                    return targets
                        .OrderBy(h =>
                        {
                           // _HP hp = h.GetComponent<_HP>();
                           return 0;// hp != null ? hp.CurrentHealth : float.MaxValue;
                        })
                        .First().transform;

                case TargetSelectionMode.Random:
                    return targets[Random.Range(0, targets.Count)].transform;

                case TargetSelectionMode.HighestPriority:
                    return targets
                        .OrderByDescending(h =>
                        {
                            TargetPriorityComponent prio = h.GetComponent<TargetPriorityComponent>();
                            if (prio == null) return 0;

                            // Если башня передала свой профиль → используем его вместо профиля цели
                            if (profile != null)
                                return prio.GetPriorityWithOverride(origin, profile);

                            return prio.GetPriority(origin);
                        })
                        .First().transform;

                default:
                    return null;
            }
        }

        public static Transform FindTarget(
            Vector3 origin,
            List<GameObject> targets,
            TargetSelectionMode mode,
            TargetPriorityProfile profile = null // 👈 новый параметр
        )
        {
            if (targets == null || targets.Count == 0) return null;
            
            switch (mode)
            {
                case TargetSelectionMode.Closest:
                    return targets
                        .OrderBy(h => Vector3.Distance(origin, h.transform.position))
                        .First().transform;

                case TargetSelectionMode.Weakest:
                    return targets
                        .OrderBy(h =>
                        {
                            //_HP hp = h.GetComponent<_HP>();
                            return 0;// hp != null ? hp.CurrentHealth : float.MaxValue;
                        })
                        .First().transform;

                case TargetSelectionMode.Random:
                    return targets[Random.Range(0, targets.Count)].transform;

                case TargetSelectionMode.HighestPriority:
                    return targets
                        .OrderByDescending(h =>
                        {
                            TargetPriorityComponent prio = h.GetComponent<TargetPriorityComponent>();
                            if (prio == null) return 0;

                            // Если башня передала свой профиль → используем его вместо профиля цели
                            if (profile != null)
                                return prio.GetPriorityWithOverride(origin, profile);

                            return prio.GetPriority(origin);
                        })
                        .First().transform;

                default:
                    return null;
            }
        }
        
    }
}