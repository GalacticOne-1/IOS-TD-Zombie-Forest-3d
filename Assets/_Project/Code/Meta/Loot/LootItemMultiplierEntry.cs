using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Location-specific override для конкретного предмета.
    /// Приоритетнее, чем LootMultiplierEntry (category-level).
    ///
    /// Resolution order: Item Override → Category Modifier → 1.0
    /// </summary>
    [Serializable]
    public struct LootItemMultiplierEntry
    {
        [FormerlySerializedAs("_item")] [SerializeField] private ItemId itemId;

        [Tooltip("Модификатор шанса появления ЭТОГО предмета в weighted pool. " +
                 "0 = никогда не выпадает, 1 = без изменений, >1 = чаще.")]
        [Min(0f)]
        [SerializeField]
        private float _weightMultiplier;

        [Tooltip("Модификатор количества ЭТОГО предмета. " +
                 "0 = полностью гасит количество (округляется вверх до 1 в раннере), 1 = без изменений, >1 = больше.")]
        [Min(0f)]
        [SerializeField]
        private float _amountMultiplier;

        public ItemId ItemId => itemId;
        public float WeightMultiplier => _weightMultiplier;
        public float AmountMultiplier => _amountMultiplier;
    }
}