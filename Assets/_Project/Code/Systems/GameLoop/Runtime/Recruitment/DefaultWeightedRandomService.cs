using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Runtime
{
    public sealed class DefaultWeightedRandomService : IWeightedRandomService
    {
        private readonly Random _random;

        public DefaultWeightedRandomService(int? seed = null)
        {
            _random = seed.HasValue
                ? new Random(seed.Value)
                : new Random();
        }

        public T PickWeighted<T>(
            IReadOnlyList<T> items,
            Func<T, int> weightSelector)
        {
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Weighted list is empty.");

            int totalWeight = 0;

            for (int i = 0; i < items.Count; i++)
            {
                int weight = weightSelector(items[i]);
                if (weight > 0)
                    totalWeight += weight;
            }

            if (totalWeight <= 0)
                throw new InvalidOperationException("All weights are zero.");

            int roll = _random.Next(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < items.Count; i++)
            {
                int weight = weightSelector(items[i]);
                if (weight <= 0)
                    continue;

                cumulative += weight;

                if (roll < cumulative)
                    return items[i];
            }

            // fallback (теоретически невозможен)
            return items[items.Count - 1];
        }

        
        public int Range(int minInclusive, int maxExclusive)
            => _random.Next(minInclusive, maxExclusive);

        
        public float Value()
            => (float)_random.NextDouble();
        
        
        public float Value01()
            => UnityEngine.Random.value;
    }
}
