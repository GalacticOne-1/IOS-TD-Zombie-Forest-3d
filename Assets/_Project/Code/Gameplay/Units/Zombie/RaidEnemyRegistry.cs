using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Реестр врагов активного рейда.
    /// Живёт внутри RaidRuntime — аналог SquadRuntime для игроков.
    ///
    /// Добавить в RaidRuntime:
    ///     public EnemyRegistry Enemies { get; } = new();
    /// </summary>
    public sealed class RaidEnemyRegistry
    {
        private readonly Dictionary<string, EnemyRuntime> _map = new();

        public IReadOnlyDictionary<string, EnemyRuntime> All => _map;

        public event Action<EnemyRuntime> OnRegistered;
        public event Action<string> OnUnregistered;

        // ── Регистрация ────────────────────────────────────────────────

        public void Register(EnemyRuntime runtime)
        {
            if (_map.TryAdd(runtime.Id, runtime))
                OnRegistered?.Invoke(runtime);
        }

        public void Unregister(string id)
        {
            if (_map.Remove(id))
                OnUnregistered?.Invoke(id);
        }

        // ── Запросы ────────────────────────────────────────────────────

        public bool TryGet(string id, out EnemyRuntime runtime)
            => _map.TryGetValue(id, out runtime);

        public int AliveCount
        {
            get
            {
                int n = 0;
                foreach (var r in _map.Values)
                    if (!r.Stats.IsDead)
                        n++;
                return n;
            }
        }

        public int TotalCount => _map.Count;

        public void Tick(float dt)
        {
            foreach (var r in _map.Values)
                r.Tick(dt);
        }

        public void Clear()
        {
            var ids = new List<string>(_map.Keys);

            foreach (var id in ids)
                Unregister(id);

            _map.Clear();
        }
    }
}