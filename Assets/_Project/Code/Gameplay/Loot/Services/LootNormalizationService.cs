
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;
using UnityEngine;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Stateless слой нормализации между генерацией и RaidLootBuffer.
    ///
    /// Отвечает за raid-wide ограничения стратегических ресурсов:
    ///   - MaxPerRaid: обрезает количество если рейдовый лимит достигнут
    ///   - MinPerRaid: используется в FulfillMinimums() после завершения генерации
    ///
    /// Поток:
    ///   Generate → Normalize(record) → buffer.AddItem(normalized)
    ///
    /// Не знает о контейнерах, слотах или seed. Только экономика.
    /// </summary>
    public sealed class LootNormalizationService
    {
        private readonly LootBalanceProfile _balanceProfile;
        private readonly RaidLootEconomyState _economyState;

        public LootNormalizationService(
            LootBalanceProfile balanceProfile,
            RaidLootEconomyState economyState)
        {
            _balanceProfile = balanceProfile;
            _economyState = economyState;
        }

        // ── Main API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Нормализует запись перед добавлением в буфер.
        ///
        /// Возвращает:
        ///   - исходный record             — нормализация не требуется
        ///   - новый record с меньшим Amount — MaxPerRaid частично достигнут
        ///   - null                         — MaxPerRaid полностью исчерпан
        ///
        /// Побочный эффект: регистрирует финальный amount в RaidLootEconomyState.
        /// </summary>
        public LootGenerationRecord Normalize(LootGenerationRecord record)
        {
            if (!record.Item.HasModule<LootModule>()) 
                return record;

            var lootModule = record.Item.LootModule;

            // Не стратегический ресурс — пропускаем без изменений
            if (!lootModule.IsStrategicResource) 
                return record;

            // Нет правила в балансе — пропускаем
            if (!_balanceProfile.TryGetStrategicRule(record.Item, out var rule)) return record;

            // Нет ограничения сверху
            if (rule.MaxPerRaid <= 0)
            {
                return record;
            }

            var alreadyTotal = _economyState.GetTotal(record.Item);

            // Лимит полностью исчерпан
            if (alreadyTotal >= rule.MaxPerRaid)
            {
                DLog.Alert(
                    $"[LootNormalization] {record.Item.name} отклонён: " +
                    $"raid-wide лимит {rule.MaxPerRaid} достигнут (уже {alreadyTotal}).", EDlogColor.YELLOW);
                return null;
            }

            // Частичное обрезание
            var remaining = rule.MaxPerRaid - alreadyTotal;
            var finalAmount = Mathf.Min(record.Amount, remaining);

            _economyState.Register(record.Item, finalAmount);

            if (finalAmount == record.Amount) 
                return record;

            DLog.Alert(
                $"[LootNormalization] {record.Item.name} обрезан: " +
                $"{record.Amount} → {finalAmount} (лимит {rule.MaxPerRaid}, уже было {alreadyTotal}).", EDlogColor.YELLOW);

            return new LootGenerationRecord(
                record.Item,
                finalAmount,
                record.Durability,
                record.ContainerRuntimeId,
                record.ContainerDefinitionId,
                record.SourceLootTableId,
                record.Context);
        }


    }
}