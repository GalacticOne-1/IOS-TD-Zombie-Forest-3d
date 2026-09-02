using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Services
{
    public static class NormalizationRules
    {
        public static List<LootGenerationRecord> Apply(
            List<LootGenerationRecord> records,
            LootBalanceProfile profile)
        {
            if (profile == null) return records;

            var tagCounts = new Dictionary<LootDropTag, int>();
            var tierCounts = new Dictionary<Tier, int>();
            var result = new List<LootGenerationRecord>(records.Count);

            foreach (var record in records)
            {
                if (!record.Item.HasModule<LootModule>())
                {
                    result.Add(record);
                    continue;
                }

                var lootModule = record.Item.GetModule<LootModule>();
                var tier = record.Item.Classification.tier;

                // Проверяем Tier Cap
                var tierCap = profile.GetTierCap(tier);
                tierCounts.TryGetValue(tier, out var currentTierCount);

                // Проверяем Tag Cap
                var tagCap = profile.GetTagCap(lootModule.DropTag);
                tagCounts.TryGetValue(lootModule.DropTag, out var currentTagCount);

                // СТУДИЙНЫЙ СТАНДАРТ ПРЕДОХРАНИТЕЛЯ:
                // Если глобальные лимиты баланса превышены — мы не просто дропаем запись, 
                // мы логируем это как предупреждение для геймдизайнера, чтобы он поправил веса в конфигах локации!
                if (currentTierCount >= tierCap || (tagCap > 0 && currentTagCount >= tagCap))
                {
                    DLog.Alert(
                        $"[LootNormalization] Элемент {record.Item.name} отсечен правилами баланса профиля. " +
                        $"Превышен лимит для Tier:{tier} или Tag:{lootModule.DropTag}. " +
                        $"Рекомендуется снизить веса этого типа предметов в LootPoolConfig.", EDlogColor.ORANGE);

                    // Опционально: здесь можно добавить инжект scrap_metal, 
                    // чтобы компенсировать пустой слот игроку.
                    continue;
                }

                // Если проверки пройдены — инкрементируем счетчики
                tierCounts[tier] = currentTierCount + 1;
                tagCounts[lootModule.DropTag] = currentTagCount + 1;

                // Срезание стака по капу
                var maxStack = profile.GetStackCap(lootModule.DropTag);
                var clampedAmount = System.Math.Min(record.Amount, maxStack);

                result.Add(clampedAmount == record.Amount
                    ? record
                    : new LootGenerationRecord(
                        record.Item,
                        clampedAmount,
                        record.Durability,
                        record.ContainerRuntimeId,
                        record.ContainerDefinitionId,
                        record.SourceLootTableId,
                        record.Context));
            }

            return result;
        }
    }
}