using System;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [Serializable]
    public struct LocationGuaranteedEntry
    {
        [SerializeField] private ItemConfig _item;

        [Header("Количество (Диапазон)")] [Min(1)] [SerializeField]
        private int _minAmount;

        [Min(1)] [SerializeField] private int _maxAmount;

        [Header("Состояние")] [SerializeField] private int _durabilityPercent;

        public ItemConfig Item => _item;
        public int MinAmount => _minAmount;
        public int MaxAmount => _maxAmount;
        public int DurabilityPercent => _durabilityPercent;

        /// <summary>
        /// Детерминированный ролл количества через переданный RNG.
        /// </summary>
        public int RollAmount(SeededRandom rng)
        {
            if (_minAmount >= _maxAmount) return _minAmount;
            return rng.NextInt(_minAmount, _maxAmount);
        }
    }

}