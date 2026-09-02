using System.Collections.Generic;
using Galactic1.Game.Meta.Stats;

namespace Galactic1.Game.Meta.Items
{
    public static class ItemStatExtensions
    {
        public static bool TryGetStat(
            this IReadOnlyList<ItemStatEntry> stats,
            StatId id,
            out float value)
        {
            foreach (var s in stats)
            {
                if (s.StatId == id)
                {
                    value = s.Value;
                    return true;
                }
            }

            value = 0;
            return false;
        }
    }
}