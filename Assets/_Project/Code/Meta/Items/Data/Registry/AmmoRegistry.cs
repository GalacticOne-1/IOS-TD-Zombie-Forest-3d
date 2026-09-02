using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup registry for items.
    /// </summary>
    public sealed class AmmoRegistry : RegistryBase<RuntimeId, ItemConfig>
    {
        // caliberId → все ItemConfig с этим калибром, отсортированные по приоритету
        private readonly Dictionary<RuntimeId, List<ItemConfig>> mapCaliber = new();

        public AmmoRegistry(IReadOnlyList<ItemConfig> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];

                if (config == null || !config.HasModule<AmmoModule>())
                    continue;

                if (config.Id == null)
                {
                    DLog.Alert($"[AmmoRegistry] Item '{config.name}' has NULL ItemId.", EDlogColor.YELLOW);
                    continue;
                }

                if (map.ContainsKey(config.Id))
                {
                    DLog.Alert($"[AmmoRegistry] Duplicate ItemId: {config.Id.name}", EDlogColor.YELLOW);
                    continue;
                }

                map.Add(config.Id, config);

                // Индексируем по калибру
                var caliberId = config.GetModule<AmmoModule>().AmmoType.Id;
                if (!mapCaliber.TryGetValue(caliberId, out var list))
                {
                    list = new List<ItemConfig>();
                    mapCaliber[caliberId] = list;
                }

                list.Add(config);
            }

            // Сортируем каждую группу по Priority (обычные первые)
            foreach (var list in mapCaliber.Values)
                list.Sort((a, b) =>
                    a.GetModule<AmmoModule>().Priority.CompareTo(
                        b.GetModule<AmmoModule>().Priority));
        }

        public IReadOnlyList<ItemConfig> GetByCaliber(RuntimeId caliberId)
            => mapCaliber.TryGetValue(caliberId, out var list)
                ? list
                : System.Array.Empty<ItemConfig>();
    }
}