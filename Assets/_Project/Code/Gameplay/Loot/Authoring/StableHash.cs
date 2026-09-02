
namespace Galactic1.RaidLoot.Core
{
    /// <summary>
    /// FNV-1a 32-bit hash.
    /// Стабилен между Unity версиями, платформами, domain reload.
    /// Детерминирован — одинаковый input всегда даёт одинаковый output.
    /// Не использует GetHashCode().
    /// </summary>
    public static class StableHash
    {
        private const uint FnvOffsetBasis = 2166136261u;
        private const uint FnvPrime = 16777619u;

        /// <summary>Hash строки в стабильный int. Null → 0.</summary>
        public static int Compute(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            unchecked
            {
                var hash = FnvOffsetBasis;
                foreach (var c in value)
                {
                    hash ^= c;
                    hash *= FnvPrime;
                }

                // Приводим к положительному int без знакового бита
                return (int)(hash & 0x7FFFFFFF);
            }
        }
    }
}