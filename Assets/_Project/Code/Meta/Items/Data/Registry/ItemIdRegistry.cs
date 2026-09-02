using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup from stable GUID to ItemId asset.
    /// Used for save/load/network restoration.
    /// </summary>
    public sealed class ItemIdRegistry
    {
        private readonly Dictionary<string, RuntimeId> map;

        public ItemIdRegistry(IReadOnlyList<ItemConfig> configs)
        {
            map = new Dictionary<string, RuntimeId>();

            foreach (var config in configs)
            {
                if (config == null || config.Id == null)
                    continue;

                var guid = config.Id.Guid;

                if (string.IsNullOrEmpty(guid))
                    continue;
                
                if (map.ContainsKey(guid))
                {
                    DLog.Alert($"[ItemIdRegistry] Duplicate GUID: {guid}", EDlogColor.RED);
                    continue;
                }

                map[guid] = config.Id;
            }
        }

        public RuntimeId Get(string guid)
        {
            return map[guid];
        }

        public bool TryGet(string guid, out RuntimeId id)
        {
            return map.TryGetValue(guid, out id);
        }
    }
}