
namespace Galactic1.RaidLoot.Services.Probability
{
    /// <summary>
    /// Математически чистая взвешенная выборка.
    /// Принимает ТОЛЬКО WeightedPool — не знает про тиры, ценности, профили.
    /// Детерминирован: один seed → один результат.
    /// </summary>
    public static class WeightedSelector
    {
        /// <summary>
        /// Выбирает один элемент из пула.
        /// Возвращает null если пул пуст или totalWeight == 0.
        /// </summary>
        public static WeightedPool.Entry? Select(
            WeightedPool pool,
            SeededRandom rng,
            out float rollResult) // для trace
        {
            rollResult = 0f;

            if (pool == null || pool.Entries.Count == 0 || pool.TotalWeight <= 0f)
                return null;

            rollResult = rng.NextFloat() * pool.TotalWeight;
            var running = 0f;

            foreach (var entry in pool.Entries)
            {
                running += entry.AdjustedWeight;
                if (rollResult <= running)
                    return entry;
            }

            return pool.Entries[pool.Entries.Count - 1]; // float precision fallback
        }
    }
}