using System.Collections.Generic;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Zombie
{
    /// <summary>
    /// Stable encirclement slot system.
    ///
    /// FIXED ARCHITECTURE:
    /// - NO immediate purge inside ReserveSlot/PeekSlot
    /// - deterministic slot allocation
    /// - explicit lifecycle cleanup only
    /// - no self-destructive dictionary entries
    /// </summary>
    public sealed class PackCoordinator : IGameService
    {
        private readonly Dictionary<string, PackGroup> _groups = new();

        // ─────────────────────────────────────────────
        // PUBLIC API
        // ─────────────────────────────────────────────

        public Vector3 PeekBestSlot(
            string targetId,
            Vector3 targetPosition,
            UnitInstance unit,
            EnemyPackDefinition pack)
        {
            var group = GetOrCreateGroup(targetId);

            if (group.TryGetSlot(unit, out var angle))
                return ToWorld(targetPosition, angle, pack.EncircleRadius);

            float freeAngle = group.FindFreeAngle(pack.SlotAngleStep);
            return ToWorld(targetPosition, freeAngle, pack.EncircleRadius);
        }

        public Vector3 ReserveSlot(
            string targetId,
            Vector3 targetPosition,
            UnitInstance unit,
            EnemyPackDefinition pack)
        {
            var group = GetOrCreateGroup(targetId);

            float angle = group.GetOrCreateSlot(unit, pack.SlotAngleStep);
            return ToWorld(targetPosition, angle, pack.EncircleRadius);
        }

        public Vector3 GetReservedSlotPosition(
            string targetId,
            Vector3 targetPosition,
            UnitInstance unit,
            EnemyPackDefinition pack)
        {
            var group = GetOrCreateGroup(targetId);

            if (!group.TryGetSlot(unit, out var angle))
                return ToWorld(targetPosition, 0f, pack.EncircleRadius);

            return ToWorld(targetPosition, angle, pack.EncircleRadius);
        }

        public void ReleaseSlot(string targetId, UnitInstance unit)
        {
            if (!_groups.TryGetValue(targetId, out var group))
                return;

            group.Release(unit);

            if (group.IsEmpty)
                _groups.Remove(targetId);
        }

        // ─────────────────────────────────────────────
        // INTERNAL
        // ─────────────────────────────────────────────

        private PackGroup GetOrCreateGroup(string targetId)
        {
            if (!_groups.TryGetValue(targetId, out var group))
            {
                group = new PackGroup();
                _groups[targetId] = group;
            }

            return group;
        }

        private static Vector3 ToWorld(Vector3 center, float angleDeg, float radius)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        }

        // ─────────────────────────────────────────────
        // GROUP
        // ─────────────────────────────────────────────

        private sealed class PackGroup
        {
            private readonly Dictionary<UnitInstance, float> _slots = new();

            public bool IsEmpty => _slots.Count == 0;

            public bool TryGetSlot(UnitInstance unit, out float angle)
                => _slots.TryGetValue(unit, out angle);

            public float GetOrCreateSlot(UnitInstance unit, float step)
            {
                if (_slots.TryGetValue(unit, out var existing))
                    return existing;

                float angle = FindFreeAngle(step);
                _slots[unit] = angle;
                return angle;
            }

            public void Release(UnitInstance unit)
            {
                _slots.Remove(unit);
            }

            public float FindFreeAngle(float step)
            {
                for (float a = 0; a < 360f; a += step)
                {
                    bool taken = false;

                    foreach (var kv in _slots)
                    {
                        if (Mathf.Abs(Mathf.DeltaAngle(kv.Value, a)) < step * 0.5f)
                        {
                            taken = true;
                            break;
                        }
                    }

                    if (!taken)
                        return a;
                }

                return Random.Range(0f, 360f);
            }
        }
    }
}