using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Gameplay;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Видимый дизайн-ассет балансировки лута.
    ///
    /// Секции:
    ///   TierCaps       — макс. предметов данного тира за одно открытие контейнера
    ///   TagCaps        — макс. предметов данного тега за одно открытие контейнера
    ///   StackCaps      — макс. количество единиц одного тега за одно открытие контейнера
    ///   StrategicRules — raid-wide caps стратегических ресурсов
    /// </summary>
    [CreateAssetMenu(
        fileName = "LootBalanceProfile",
        menuName = "Game Configs/Loot/Loot Balance Profile")]
    public sealed class LootBalanceProfile : ScriptableObject
    {
        [Header("Tier Caps — сколько предметов данного тира за одно открытие")]
        [SerializeField] private List<TierCapRule> _tierCaps = new();

        [Header("Drop Tag Caps — сколько предметов данного тега за одно открытие")]
        [SerializeField] private List<TagCapRule> _tagCaps = new();

        [Header("Stack Caps — максимальное количество единиц одного тега за одно открытие")]
        [SerializeField] private List<TagStackRule> _stackCaps = new();

        [Header("Strategic Resource Caps — max за весь рейд")]
        [Tooltip("Применяются в LootNormalizationService. Работают across всех контейнеров и location guaranteed.")]
        [SerializeField] private List<StrategicResourceRule> _strategicRules = new();

        // ── Per-container rule structs ────────────────────────────────────────

        [Serializable]
        public struct TierCapRule
        {
            public Tier Tier;
            [Tooltip("Максимум предметов этого тира за одно открытие контейнера.")]
            public int MaxPerOpen;
            [TextArea(1, 2)] public string DesignNote;
        }

        [Serializable]
        public struct TagCapRule
        {
            public LootDropTag Tag;
            public int MaxPerOpen;
            [TextArea(1, 2)] public string DesignNote;
        }

        [Serializable]
        public struct TagStackRule
        {
            public LootDropTag Tag;
            public int MaxStack;
            [TextArea(1, 2)] public string DesignNote;
        }

        // ── Per-container queries ─────────────────────────────────────────────

        public int GetTierCap(Tier tier)
        {
            foreach (var r in _tierCaps)
                if (r.Tier == tier) return r.MaxPerOpen;
            return int.MaxValue;
        }

        public int GetTagCap(LootDropTag tag)
        {
            foreach (var r in _tagCaps)
                if (r.Tag == tag) return r.MaxPerOpen;
            return int.MaxValue;
        }

        public int GetStackCap(LootDropTag tag)
        {
            foreach (var r in _stackCaps)
                if (r.Tag == tag) return r.MaxStack;
            return 999;
        }

        // ── Raid-wide strategic queries ───────────────────────────────────────

        /// <summary>
        /// Возвращает правило для предмета, если он является стратегическим ресурсом.
        /// Возвращает false если правило не задано.
        /// </summary>
        public bool TryGetStrategicRule(ItemConfig item, out StrategicResourceRule rule)
        {
            foreach (var r in _strategicRules)
            {
                if (GameContent.Items.TryGet(r.ItemId, out var _item) && _item == item)
                {
                    rule = r;
                    return true;
                }
            }
            rule = default;
            return false;
        }

        public IReadOnlyList<StrategicResourceRule> StrategicRules => _strategicRules;
    }
}