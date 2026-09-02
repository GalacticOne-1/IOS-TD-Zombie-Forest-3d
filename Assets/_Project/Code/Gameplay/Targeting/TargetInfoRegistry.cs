using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// O(1) collider → ITargetInfo lookup + O(1) TargetId → ITargetInfo lookup.
    ///
    /// ИЗМЕНЕНИЯ ДЛЯ SIEGE (аддитивные, внешний API не менялся):
    ///   — добавлен TryGetById() для универсального разрешения TargetId
    ///     (нужен FindHostileById, чтобы находить здания, не всегда попадающие
    ///     в PhysicsPerception.GetVisibleTargets());
    ///   — multi-collider ref-counting через единую структуру IdEntry, чтобы
    ///     Unregister() одного из нескольких коллайдеров одного target не
    ///     преждевременно вымывал его из id-индекса, и чтобы коллизия двух
    ///     разных target'ов под одним TargetId не приводила к смешению
    ///     refcount (см. AcquireId/ReleaseId).
    /// </summary>
    public static class TargetInfoRegistry
    {
        private static readonly Dictionary<Collider, ITargetInfo> _map = new(128);

        /// <summary>Одна запись = один "владелец" TargetId. RefCount привязан
        /// именно к владеющему объекту, а не к строке id самой по себе —
        /// это исключает возможность смешения счётчика двух разных объектов
        /// при коллизии id.</summary>
        private sealed class IdEntry
        {
            public ITargetInfo Target;
            public int RefCount;
        }

        private static readonly Dictionary<string, IdEntry> _idEntries = new(128);

        // ── Lifecycle ─────────────────────────────────────────────────────

        public static void Clear()
        {
            _map.Clear();
            _idEntries.Clear();
        }

        // ── Registration ──────────────────────────────────────────────────

        public static void Register(Collider collider, ITargetInfo target)
        {
            if (collider == null || target == null) return;

            if (_map.TryGetValue(collider, out var existing))
            {
                if (existing == target)
                    return; // повторная регистрация того же collider→target — no-op

                Debug.LogWarning(
                    $"[TargetInfoRegistry] Collider '{collider.name}' is already " +
                    $"registered to a different target ('{existing}') and will be " +
                    $"overwritten by '{target}'. Check for duplicate Bind() calls " +
                    $"or missing Unregister() on pool return.");

                ReleaseId(existing); // collider переходит на новый target — старый теряет один ref
            }

            _map[collider] = target;
            AcquireId(target);
        }

        public static void Unregister(Collider collider)
        {
            if (collider == null) return;

            if (_map.TryGetValue(collider, out var target))
                ReleaseId(target);

            _map.Remove(collider);
        }

        // ── Lookup ────────────────────────────────────────────────────────

        public static bool TryGet(Collider collider, out ITargetInfo target)
        {
            if (!_map.TryGetValue(collider, out target))
                return false;

            if (target == null)
            {
                _map.Remove(collider);
                return false;
            }

            return true;
        }

        /// <summary>NEW — глобальный id-индекс, не зависящий от PhysicsPerception scan.
        /// Нужен FindHostileById для разрешения зданий (HQ/стены), которые могут
        /// не оказаться в списке видимых целей конкретного юнита.</summary>
        public static bool TryGetById(string targetId, out ITargetInfo target)
        {
            if (string.IsNullOrEmpty(targetId) || !_idEntries.TryGetValue(targetId, out var entry))
            {
                target = null;
                return false;
            }

            target = entry.Target;
            if (target == null) // Unity pseudo-null guard
            {
                _idEntries.Remove(targetId);
                return false;
            }

            return true;
        }

        // ── Internal ref-counting ────────────────────────────────────────

        private static void AcquireId(ITargetInfo target)
        {
            var id = target.TargetId;
            if (string.IsNullOrEmpty(id)) return;

            if (_idEntries.TryGetValue(id, out var entry))
            {
                if (!ReferenceEquals(entry.Target, target))
                {
                    // TargetId collision: два разных ITargetInfo с одним id.
                    // По контракту TargetInfoBase.Initialize() (GUID-based
                    // fallback) это не должно происходить. Намеренно НЕ
                    // смешиваем refcount двух разных объектов под одним id —
                    // первый зарегистрировавшийся остаётся authoritative
                    // владельцем id; второй по-прежнему отслеживается в _map
                    // (collider-based lookup работает), но не экспонируется
                    // через id-based lookup, пока конфликт не разрешится
                    // (например через смерть/Unregister первого).
                    Debug.LogWarning(
                        $"[TargetInfoRegistry] TargetId collision: '{id}' already owned by " +
                        $"'{entry.Target}', cannot also register '{target}'. " +
                        $"TargetId generation must guarantee uniqueness upstream.");
                    return;
                }

                entry.RefCount++;
                return;
            }

            _idEntries[id] = new IdEntry { Target = target, RefCount = 1 };
        }

        private static void ReleaseId(ITargetInfo target)
        {
            var id = target?.TargetId;
            if (string.IsNullOrEmpty(id)) return;
            if (!_idEntries.TryGetValue(id, out var entry)) return;

            // Коллайдер, отклонённый в AcquireId из-за коллизии, никогда не был
            // засчитан в этот entry — его Unregister не должен decrement-ить чужой счётчик.
            if (!ReferenceEquals(entry.Target, target)) return;

            entry.RefCount--;
            if (entry.RefCount <= 0)
                _idEntries.Remove(id);
        }
    }
}
