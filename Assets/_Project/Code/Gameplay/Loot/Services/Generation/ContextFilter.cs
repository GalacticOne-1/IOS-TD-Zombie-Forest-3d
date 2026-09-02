
using System.Collections.Generic;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Services.Rules;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Фильтрует пул слота по явным предикатам.
    /// Каждый предикат — отдельный именованный метод.
    /// Нет скрытых правил. Дизайнер видит что фильтруется.
    /// </summary>
    public static class ContextFilter
    {
        public static List<LootWeightedEntry> Filter(
            LootWeightedEntry[] pool,
            TierLimitResolver.TierLimits tierLimits)
        {
            var result = new List<LootWeightedEntry>(pool.Length);

            foreach (var entry in pool)
            {
                if (entry.Item == null ||
                    !entry.Item.HasModule<LootModule>())
                    continue;

                var loot = entry.Item.GetModule<LootModule>();

                if (!tierLimits.Allows(entry.Item.Classification.tier))
                    continue;

                result.Add(entry);
            }

            return result;
        }

    }
}