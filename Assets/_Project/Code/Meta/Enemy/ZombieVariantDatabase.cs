using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(
        fileName = "ZombieVariantDatabase",
        menuName = "Game Configs/Enemy/Zombie Variant Database")]
    public sealed class ZombieVariantDatabase : ScriptableObject
    {
        [SerializeField]
        private List<EnemyArchetypeConfig> _variants;

        private Dictionary<EnemyId, EnemyArchetypeConfig> _map;

        public void Build()
        {
            _map = new();

            foreach (var cfg in _variants)
            {
                if (cfg == null)
                    continue;

                if (_map.ContainsKey(cfg.Id))
                {
                    Debug.LogError(
                        $"Duplicate zombie config id: {cfg.Id}");

                    continue;
                }

                _map.Add(cfg.Id, cfg);
            }
        }

        public EnemyArchetypeConfig GetById(EnemyId id)
        {
            if (_map == null)
                Build();

            return _map.GetValueOrDefault(id);
        }

        public IReadOnlyList<EnemyArchetypeConfig> All => _variants;
    }
}