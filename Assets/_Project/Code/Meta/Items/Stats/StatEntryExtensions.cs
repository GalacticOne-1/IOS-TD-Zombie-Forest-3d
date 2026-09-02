using System.Collections.Generic;
using Galactic1.Game.Meta.Stats;

namespace Galactic1.Game.Meta.Items
{
    public static class StatEntryExtensions
    {
        public static bool TryGetStat(
            this List<ItemStatEntry> stats,
            StatId statId,
            out float value)
        {
            foreach (var s in stats)
            {
                if (s.StatId == statId)
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