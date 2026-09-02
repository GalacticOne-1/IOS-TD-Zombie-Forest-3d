
using System.Collections.Generic;
using Galactic1.Code.Core;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Преобразует RaidLootBuffer в List&lt;RaidRewardLootData&gt; для RaidResultProxy.
    ///
    /// Группирует записи по ItemConfig, суммирует Amount.
    /// Durability: для нестакаемых предметов берётся из каждой записи отдельно,
    /// для стакаемых (maxStack > 1) — 0 (ресурсы не имеют прочности).
    ///
    /// Вызывается исключительно из RaidRuntime.CalculateResult().
    /// </summary>
    public sealed class LootResultMapper
    {
        public List<RaidRewardLootData> Map(RaidLootBuffer buffer)
        {
            var result = new List<RaidRewardLootData>();

            foreach (var entry in buffer.GetAll())
            {
                var item = entry.Item;

                if (item.IsStackable)
                {
                    // Стакаемые — группируем, прочность не нужна
                    var existing = result.Find(r => r.ConfigId == item.Id.Guid);
                    if (existing != null)
                        existing.Amount += entry.Amount;
                    else
                        result.Add(new RaidRewardLootData
                        {
                            ConfigId = item.Id.Guid,
                            Amount = entry.Amount,
                            Durability = 0,
                            Item = item
                        });
                }
                else
                {
                    // Нестакаемые — каждая запись отдельной строкой с прочностью
                    result.Add(new RaidRewardLootData
                    {
                        ConfigId = item.Id.Guid,
                        Amount = entry.Amount,
                        Durability = entry.Record.Durability,
                        Item = item
                    });
                }
            }

            return result;
        }
    }
}