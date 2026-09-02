
using System;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Запись в взвешенном пуле слота.
    /// Вес — не вероятность, а относительный шанс выпадения среди конкурентов в пуле.
    /// Нормализация весов выполняется в WeightedSelector.
    /// </summary>
    [Serializable]
    public struct LootWeightedEntry
    {
        public ItemConfig Item;

        [Tooltip("Относительный вес. Больше вес — чаще выпадает. Не вероятность.")] [Min(0.01f)]
        public float Weight;

        [Tooltip("Минимальное количество.")] public int MinAmount;

        [Tooltip("Максимальное количество.")] public int MaxAmount;

        [Tooltip("Мин. прочность в % от maxDurability.")]
        public int MinDurabilityPercent;

        [Tooltip("Макс. прочность в % от maxDurability.")]
        public int MaxDurabilityPercent;
    }
}