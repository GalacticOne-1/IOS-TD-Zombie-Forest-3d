
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Systems.GameSession.WorldMap;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Корневая конфигурация всех локаций и карты мира.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationsConfigs", menuName = "Game Configs/World Map/Locations Configs")]
    public class LocationsConfigs : ScriptableObject
    {
        [field: SerializeField] public WorldMapConfig WorldMapConfig { get; private set; }
        
        
        [field: Tooltip("id 0 закреплен за HomeConfig")]
        [field: SerializeField] public List<LocationConfig> Locations { get; private set; }
        
        private Dictionary<LocationId, LocationConfig> _cache;
        // mapNode и LocationConfig связываются по configId

        
        
        public LocationConfig GetConfig(LocationId id)
        {
            if (_cache == null)
                BuildCache();

            if (!_cache.TryGetValue(id, out var config))
            {
                Debug.LogError($"LocationConfig not found for id: {id}");
                return null;
            }

            return config;
        }
        
        private void BuildCache()
        {
            _cache = new Dictionary<LocationId, LocationConfig>();

            var l = Locations.Count;
            for (int i = 0; i < l; i++)
            {
                if (Locations[i].Id == null)
                {
                    Debug.LogError($"LocationConfig without LocationId: {Locations[i].name}");
                    continue;
                }

                Locations[i].SetIndex = i;
                _cache[Locations[i].Id] = Locations[i];
            }
        }
    }
}