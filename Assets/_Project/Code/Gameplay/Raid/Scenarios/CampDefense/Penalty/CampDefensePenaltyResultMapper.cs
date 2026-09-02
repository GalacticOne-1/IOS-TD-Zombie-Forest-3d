using System.Collections.Generic;
using Galactic1.Code.Core;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Преобразует CampDefensePenaltyResult в List&lt;RaidPenaltyLossData&gt; для RaidResultProxy.
    /// Аналог LootResultMapper — только для штрафа, а не награды.
    ///
    /// Группировка не нужна: Calculator уже отдаёт одну запись на предмет
    /// (штраф считается по существующим стакам склада, без дублей).
    ///
    /// Вызывается исключительно из CampDefenseScenario.BuildRaidResult().
    /// </summary>
    public sealed class CampDefensePenaltyResultMapper
    {
        public List<RaidPenaltyLossData> Map(CampDefensePenaltyResult result)
        {
            var mapped = new List<RaidPenaltyLossData>();

            if (result == null || !result.HasPenalty)
                return mapped;

            foreach (var item in result.Items)
            {
                mapped.Add(new RaidPenaltyLossData
                {
                    ConfigId = item.Item.Id.Guid,
                    Amount = item.Amount,
                    Item = item.Item
                });
            }

            return mapped;
        }
    }
}