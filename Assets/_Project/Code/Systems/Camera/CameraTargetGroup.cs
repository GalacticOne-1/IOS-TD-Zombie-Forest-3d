using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Cameras
{
    /// <summary>
    /// Aggregates multiple targets to calculate a stable focus point for RTS camera.
    /// Used ONLY for manual focus (no auto-follow).
    /// </summary>
    public sealed class CameraTargetGroup : IGameService
    {
        private readonly List<Transform> targets = new();

        public bool HasTargets => targets.Count > 0;

        public void Clear()
        {
            targets.Clear();
        }

        public void Add(Transform target)
        {
            if (target == null)
                return;

            if (!targets.Contains(target))
                targets.Add(target);
        }

        public void Remove(Transform target)
        {
            if (target == null)
                return;

            targets.Remove(target);
        }

        public Vector3 GetCenter()
        {
            if (targets.Count == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null)
                    continue;

                sum += targets[i].position;
                count++;
            }

            if (count == 0)
                return Vector3.zero;

            sum /= count;
            sum.z = 0f;

            return sum;
        }
    }
}