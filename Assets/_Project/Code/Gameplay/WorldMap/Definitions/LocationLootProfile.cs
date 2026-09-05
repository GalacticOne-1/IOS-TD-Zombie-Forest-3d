using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.RaidLoot.Definition
{
    /// <summary>
    /// Runtime-разрешение balance-модификаторов лута для локации.
    ///
    /// Два уровня:
    ///   Category multiplier — default-правило для всех предметов категории.
    ///   Item override        — location-specific исключение для одного предмета,
    ///                           приоритетнее category.
    ///
    /// Resolution order: Item Override → Category Modifier → 1.0
    ///
    /// WeightMultiplier влияет ТОЛЬКО на relative selection weight внутри weighted pool
    /// (не применяется к guaranteed loot).
    /// AmountMultiplier влияет на итоговое количество и применяется везде:
    /// container guaranteed, container slots, location guaranteed.
    /// </summary>
    public sealed class LocationLootProfile
    {
        private readonly Dictionary<LootEconomyCategory, LootMultiplierData> _categoryMultipliers;
        private readonly Dictionary<string, LootMultiplierData> _itemMultipliers;

        public LocationId LocationId { get; }

        
        public LocationLootProfile(
            LocationId locationId,
            IEnumerable<LootMultiplierEntry> categoryEntries,
            IEnumerable<LootItemMultiplierEntry> itemEntries)
        {
            LocationId = locationId;

            _categoryMultipliers = BuildCategoryDictionary(categoryEntries);
            _itemMultipliers = BuildItemDictionary(itemEntries);
        }

        // ── Category-level API (существующий, не удалять) ──────────────────────

        public float GetWeightMultiplier(LootEconomyCategory category)
        {
            return _categoryMultipliers.TryGetValue(category, out var data)
                ? data.WeightMultiplier
                : 1f;
        }

        public float GetAmountMultiplier(LootEconomyCategory category)
        {
            return _categoryMultipliers.TryGetValue(category, out var data)
                ? data.AmountMultiplier
                : 1f;
        }

        // ── Item-aware API (новый, приоритетный) ────────────────────────────────

        /// <summary>
        /// Resolution: item override → category multiplier → 1.0.
        /// Это тот метод, который должны использовать все generation paths.
        /// </summary>
        public float GetWeightMultiplier(ItemConfig item)
        {
            if (item == null)
                return 1f;

            if (TryGetItemOverride(item, out var data))
                return data.WeightMultiplier;

            return GetWeightMultiplier(item.Classification.economyCategory);
        }

        public float GetAmountMultiplier(ItemConfig item)
        {
            if (item == null)
                return 1f;

            if (TryGetItemOverride(item, out var data))
                return data.AmountMultiplier;

            return GetAmountMultiplier(item.Classification.economyCategory);
        }

        private bool TryGetItemOverride(ItemConfig item, out LootMultiplierData data)
        {
            data = default;

            if (item?.Id == null || string.IsNullOrEmpty(item.Id.Guid))
                return false;

            return _itemMultipliers.TryGetValue(item.Id.Guid, out data);
        }

        // ── Dictionary construction ──────────────────────────────────────────────

        private static Dictionary<LootEconomyCategory, LootMultiplierData> BuildCategoryDictionary(
            IEnumerable<LootMultiplierEntry> entries)
        {
            var result = new Dictionary<LootEconomyCategory, LootMultiplierData>();
            if (entries == null) return result;

            foreach (var entry in entries)
            {
                // Существующее поведение (ToDictionary) не менялось раньше —
                // сохраняем "последний выигрывает", чтобы не менять текущую семантику.
                result[entry.Category] = new LootMultiplierData(
                    entry.WeightMultiplier,
                    entry.AmountMultiplier);
            }

            return result;
        }

        private static Dictionary<string, LootMultiplierData> BuildItemDictionary(
            IEnumerable<LootItemMultiplierEntry> entries)
        {
            var result = new Dictionary<string, LootMultiplierData>();
            if (entries == null) return result;

            foreach (var entry in entries)
            {
                if (entry.ItemId == null || entry.ItemId == null || string.IsNullOrEmpty(entry.ItemId.Guid))
                    continue;

                var guid = entry.ItemId.Guid;

                if (result.ContainsKey(guid))
                {
                    Debug.LogError(
                        $"[LocationLootProfile] Duplicate item override for '{entry.ItemId.name}' " +
                        "(guid collision). Keeping the first entry, ignoring the rest. " +
                        "Fix this in the LocationLootProfileConfig asset.");
                    continue;
                }

                result[guid] = new LootMultiplierData(
                    entry.WeightMultiplier,
                    entry.AmountMultiplier);
            }

            return result;
        }

        private readonly struct LootMultiplierData
        {
            public readonly float WeightMultiplier;
            public readonly float AmountMultiplier;

            public LootMultiplierData(float weightMultiplier, float amountMultiplier)
            {
                WeightMultiplier = weightMultiplier;
                AmountMultiplier = amountMultiplier;
            }
        }
    }
}