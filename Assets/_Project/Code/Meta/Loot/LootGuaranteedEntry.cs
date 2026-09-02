using System;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [Serializable]
    public struct LootGuaranteedEntry
    {
        [SerializeField] private ItemConfig _item;

        [Min(1)] [SerializeField]
        private int _minAmount;

        [Min(1)] [SerializeField] private int _maxAmount;

        [Header("Состояние")] [SerializeField]
        private int _durabilityPercent;

        public ItemConfig Item => _item;
        public int MinAmount => _minAmount;
        public int MaxAmount => _maxAmount;
        public int DurabilityPercent => _durabilityPercent;

        /// <summary>
        /// Вычисляет финальное количество на основе детерминированного RNG.
        /// </summary>
        public int RollAmount(SeededRandom rng)
        {
            // Fail-safe guard: если дизайнер указал min >= max, просто возвращаем min
            if (_minAmount >= _maxAmount)
                return _minAmount;

            return rng.NextInt(_minAmount, _maxAmount);
        }
    }
}