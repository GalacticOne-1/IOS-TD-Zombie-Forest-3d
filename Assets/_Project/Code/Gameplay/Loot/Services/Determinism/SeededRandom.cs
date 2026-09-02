
namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Изолированный RNG на основе seed.
    /// Не трогает Unity Random.state — полностью детерминирован.
    /// Использует xorshift32 — быстро, без аллокаций.
    /// </summary>
    public sealed class SeededRandom
    {
        private uint _state;

        public SeededRandom(int seed)
        {
            _state = seed == 0 ? 2463534242u : (uint)seed;
        }

        /// <summary>Returns value in [0, 1].</summary>
        public float NextFloat()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (_state & 0x7FFFFFFF) / (float)0x80000000;
        }

        /// <summary>Returns integer in [min, max].</summary>
        public int NextInt(int min, int max)
        {
            if (min >= max) return min;
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return min + (int)(_state % (uint)(max - min + 1));
        }
    }
}