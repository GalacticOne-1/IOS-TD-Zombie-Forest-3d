using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Gameplay.Units.Repositories;
using Galactic1.Code.Gameplay.Units.Zombie;

namespace Galactic1.Code.Gameplay.Enemies.Repositories
{
    /// <summary>
    /// Specialized repository — filtered view для enemy-юнитов.
    ///
    /// Уровень 2 архитектуры. НЕ является storage — хранит только
    /// набор id, а сами объекты берёт из UnitSceneRepository.
    ///
    /// Используется:
    ///   AI Director, Wave Manager, Spawn Balancing, Difficulty Systems.
    ///
    /// Gameplay interaction systems (Damage, AoE, Perception)
    /// НЕ должны использовать этот репозиторий.
    /// </summary>
    public sealed class EnemyRepository : IGameService
    {
        private readonly UnitSceneRepository _scene;
        private readonly HashSet<string> _enemyIds = new();

        public EnemyRepository(UnitSceneRepository scene)
        {
            _scene = scene;
        }

        // ── Registration ──────────────────────────────────────────

        public void Register(string id)
        {
            if (!string.IsNullOrEmpty(id))
                _enemyIds.Add(id);
        }

        public void Unregister(string id)
        {
            _enemyIds.Remove(id);
        }

        // ── Filtered access ───────────────────────────────────────

        /// <summary>
        /// Все активные enemy-юниты сцены.
        /// </summary>
        public IEnumerable<EnemyInstance> ActiveEnemies =>
            _enemyIds
                .Select(id => _scene.TryGet(id))
                .Where(r => r.done && r.instance is EnemyInstance)
                .Select(r => (EnemyInstance)r.instance);

        public int Count => _enemyIds.Count;

        public (bool done, EnemyInstance instance) TryGet(string id)
        {
            if (!_enemyIds.Contains(id))
                return (false, null);

            var result = _scene.TryGet(id);
            return (result.done && result.instance is EnemyInstance z)
                ? (true, z)
                : (false, null);
        }

        public void Clear()
        {
            _enemyIds.Clear();
        }
    }
}