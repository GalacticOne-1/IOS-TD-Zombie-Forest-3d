
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Какие зоны сейчас ждёт активный объектив. Триггер не сгорает, пока его никто не ждёт.</summary>
    public static class TutorialZoneTracker
    {
        private static readonly Dictionary<TutorialTargetId, int> Awaited = new();

        public static void Add(TutorialTargetId id)
        {
            if (id == null) return;
            Awaited.TryGetValue(id, out var n);
            Awaited[id] = n + 1;
        }

        public static void Remove(TutorialTargetId id)
        {
            if (id == null || !Awaited.TryGetValue(id, out var n)) return;
            if (n <= 1) Awaited.Remove(id); else Awaited[id] = n - 1;
        }

        public static bool IsAwaited(TutorialTargetId id) => id != null && Awaited.ContainsKey(id);
    }
}