
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.RaidLoot.Definitions
{
    /// <summary>
    /// Реестр всех визуальных наборов контейнеров.
    /// Один глобальный ассет на проект.
    ///
    /// Поиск по RuntimeId через Dictionary — O(1).
    /// Dictionary строится лениво при первом обращении.
    /// </summary>
    [CreateAssetMenu(
        fileName = "LootContainerVisualDatabase",
        menuName = "Game Configs/Loot/Container Visual Database")]
    public sealed class LootContainerVisualDatabase : ScriptableObject
    {
        [SerializeField] private List<LootContainerVisualConfig> _visuals = new();

        private Dictionary<LootVisualId, LootContainerVisualConfig> _cache;

        public IReadOnlyList<LootContainerVisualConfig> Visuals => _visuals;

        /// <summary>
        /// Ищет визуальный набор по RuntimeId.
        /// Возвращает null если не найдено — логируем предупреждение на уровне View.
        /// </summary>
        public LootContainerVisualConfig Get(LootVisualId id)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out var def) ? def : null;
        }

        public bool TryGet(LootVisualId id, out LootContainerVisualConfig config)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out config);
        }

        private void EnsureCache()
        {
            if (_cache != null) return;

            _cache = new Dictionary<LootVisualId, LootContainerVisualConfig>(_visuals.Count);
            foreach (var visual in _visuals)
            {
                if (visual == null || visual.Id == null) continue;
                _cache[visual.Id] = visual;
            }
        }

        // Сбрасываем кэш при изменении ассета в редакторе
        private void OnValidate() => _cache = null;
    }
}