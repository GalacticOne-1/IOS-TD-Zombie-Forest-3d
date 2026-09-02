using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Repositories;

namespace Galactic1.Code.Gameplay.Survivors.Repositories
{
    /// <summary>
    /// Specialized repository — filtered view для survivor-юнитов.
    ///
    /// Уровень 2 архитектуры. НЕ является storage — хранит только
    /// набор id, а сами объекты берёт из UnitSceneRepository.
    ///
    /// Используется:
    ///   Squad UI, Player Commands, Recruitment, Formation Systems.
    ///
    /// Gameplay interaction systems (Damage, AoE, Perception)
    /// НЕ должны использовать этот репозиторий.
    /// </summary>
    public sealed class SurvivorRepository : IGameService
    {
        private readonly UnitSceneRepository _scene;
        private readonly HashSet<string> _survivorIds = new();

        public SurvivorRepository(UnitSceneRepository scene)
        {
            _scene = scene;
        }

        // ── Registration ──────────────────────────────────────────

        public void Register(string id)
        {
            if (!string.IsNullOrEmpty(id))
                _survivorIds.Add(id);
        }

        public void Unregister(string id)
        {
            _survivorIds.Remove(id);
        }

        // ── Filtered access ───────────────────────────────────────

        /// <summary>
        /// Все живые survivor-юниты сцены.
        /// Автоматически пропускает юниты, уже удалённые из сцены.
        /// </summary>
        public IEnumerable<SurvivorInstance> ActiveSurvivors =>
            _survivorIds
                .Select(id => _scene.TryGet(id))
                .Where(r => r.done && r.instance is SurvivorInstance)
                .Select(r => (SurvivorInstance)r.instance);

        public (bool done, SurvivorInstance instance) TryGet(string id)
        {
            if (!_survivorIds.Contains(id))
                return (false, null);

            var result = _scene.TryGet(id);
            return (result.done && result.instance is SurvivorInstance s)
                ? (true, s)
                : (false, null);
        }

        public void Clear()
        {
            _survivorIds.Clear();
        }
    }
}