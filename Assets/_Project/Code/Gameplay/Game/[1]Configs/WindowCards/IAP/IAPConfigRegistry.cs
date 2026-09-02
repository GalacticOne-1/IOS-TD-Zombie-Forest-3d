using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Shop;
using UnityEngine;

namespace Galactic1.Configs
{
    public class IAPConfigRegistry
    {
        // Словари для быстрого поиска по id
        public readonly Dictionary<IAPId, IAPConfig> _configs = new();


        public IAPConfigRegistry(Dictionary<string, ScriptableObject> rawConfigs)
        {
            foreach (var config in rawConfigs.Values)
            {
                if(config is IAPConfig iapConfig)
                {
                    if (!_configs.ContainsKey(iapConfig.Id))
                        _configs.Add(iapConfig.Id, iapConfig);
                    else
                        Debug.LogError($"Duplicate _IAPConfig_ id: {iapConfig.Id}");
                }
            }
        }
        
        
        // --------------------------
        // Получение конкретного конфига по id
        // --------------------------
        public IAPConfig Get(IAPId id)
        {
            if (_configs.TryGetValue(id, out var config)) return config;
            DLog.Alert($"_IAPConfig_ with id '{id}' not found!", EDlogColor.RED);
            return null;
        }
    }   
}