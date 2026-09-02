using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1.Configs
{
    public class StructureConfigRegistry
    {
        // Словари для быстрого поиска по id
        public readonly Dictionary<string, BuildableConfig> _configs = new();
        
        public StructureConfigRegistry(Dictionary<string, ScriptableObject> rawConfigs)
        {
            foreach (var config in rawConfigs.Values)
            {
                if(config is BuildableConfig buildableConfig)
                {
                    if (!_configs.ContainsKey(buildableConfig.ConfigId))
                        _configs.Add(buildableConfig.ConfigId, buildableConfig);
                    else
                        Debug.LogError($"Duplicate StructureConfigs id: {buildableConfig.ConfigId}");
                }
            }
        }
        
        
        // --------------------------
        // Получение конкретного конфига по id
        // --------------------------
        public BuildableConfig Get(string configId)
        {
            if (_configs.TryGetValue(configId, out var config)) return config;
            DLog.Alert($"StructureConfigs with id '{configId}' not found!", EDlogColor.RED);
            return null;
        }
    }
}