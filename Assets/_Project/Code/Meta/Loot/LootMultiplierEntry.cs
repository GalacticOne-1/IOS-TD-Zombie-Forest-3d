using System;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [Serializable]
    public struct LootMultiplierEntry
    {
        [SerializeField]
        private LootEconomyCategory _category;

        [SerializeField]
        private float _weightMultiplier;

        [SerializeField]
        private float _amountMultiplier;

        public LootEconomyCategory Category => _category;

        /// <summary>
        /// Модификатор шанса появления категории.
        /// Используется при построении WeightedPool.
        /// </summary>
        public float WeightMultiplier => _weightMultiplier;

        /// <summary>
        /// Модификатор количества выпавшего предмета.
        /// Используется в QuantityRoller.
        /// </summary>
        public float AmountMultiplier => _amountMultiplier;
    }
}