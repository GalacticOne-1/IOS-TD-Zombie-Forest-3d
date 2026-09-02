using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Core.Repositories;

namespace Galactic1.Code.Gameplay.Units.Repositories
{
    /// <summary>
    /// Canonical scene registry для ВСЕХ юнитов сцены.
    ///
    /// Уровень 1 архитектуры — primary ownership registry.
    /// Хранит UnitInstance (базовый тип), поэтому подходит для:
    ///   SurvivorInstance, ZombieInstance, NPCInstance, BossInstance,
    ///   TurretInstance, SummonInstance — любых будущих юнитов.
    ///
    /// Используется:
    ///   Damage, AoE, Perception, Targeting, Threat, Audio, LOS, NavAvoidance.
    ///
    /// НЕ используется:
    ///   Squad UI, AI Director, Recruitment — они идут в specialized repos.
    /// </summary>
    public sealed class UnitSceneRepository : IRepository<ISceneEntity>, IGameService
    {
        private readonly Dictionary<string, ISceneEntity> _units = new();

        public IReadOnlyDictionary<string, ISceneEntity> All => _units;

        public void Register(string withId, ISceneEntity instance)
        {
            if (string.IsNullOrEmpty(withId) || instance == null)
                return;

            if (_units.ContainsKey(withId))
                return;

            _units.Add(withId, instance);
        }

        public void Unregister(string withId, ISceneEntity instance)
        {
            if (string.IsNullOrEmpty(withId))
                return;

            _units.Remove(withId);
        }

        public (bool done, ISceneEntity instance) TryGet(string id)
        {
            return (_units.TryGetValue(id, out var instance), instance);
        }

        public void Clear()
        {
            _units.Clear();
        }
    }
}