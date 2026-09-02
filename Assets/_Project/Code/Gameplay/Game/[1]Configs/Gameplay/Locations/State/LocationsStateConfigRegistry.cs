using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1.Configs
{
    public class LocationsStateConfigRegistry
    {
        // Словари для быстрого поиска по id
        public readonly Dictionary<int, LocationStateConfigs> _configs = new();


        public LocationsStateConfigRegistry(Dictionary<string, ScriptableObject> rawConfigs)
        {
            foreach (var config in rawConfigs.Values)
            {
                if(config is LocationStateConfigs iapConfig)
                {
                    if (!_configs.ContainsKey(iapConfig.LocationId))
                        _configs.Add(iapConfig.LocationId, iapConfig);
                    else
                        Debug.LogError($"Duplicate LocationStateConfigs id: {iapConfig.LocationId}");
                }
            }
        }
        
        
        // --------------------------
        // Получение конкретного конфига по id
        // --------------------------
        public LocationStateConfigs Get(int configId)
        {
            if (_configs.TryGetValue(configId, out var config)) return config;
            DLog.Alert($"LocationStateConfigs with id '{configId}' not found!", EDlogColor.RED);
            return null;
        }
    }
}