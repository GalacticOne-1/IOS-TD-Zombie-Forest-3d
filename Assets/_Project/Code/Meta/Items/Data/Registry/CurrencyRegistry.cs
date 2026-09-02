using System.Collections.Generic;
using Galactic1.Game.Meta.Economy;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup registry for items.
    /// </summary>
    public sealed class CurrencyRegistry : RegistryBase<RuntimeId, CurrencyConfig>
    {
        public CurrencyRegistry(IReadOnlyList<CurrencyConfig> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];

                if (config == null)
                {
                    DLog.Alert($"[CurrencyRegistry] Null config at index {i}", EDlogColor.YELLOW);
                    continue;
                }

                if (config.Id == null)
                {
                    DLog.Alert($"[CurrencyRegistry] Item '{config.name}' has NULL ItemId.", EDlogColor.YELLOW);
                    continue;
                }

                if (map.ContainsKey(config.Id))
                {
                    DLog.Alert($"[ItemRegistry] Duplicate ItemId detected: {config.Id.name}", EDlogColor.YELLOW);
                    continue;
                }

                map.Add(config.Id, config);
            }
        }
        
    }
}