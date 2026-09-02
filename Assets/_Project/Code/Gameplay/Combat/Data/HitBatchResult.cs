using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Hit;

namespace Galactic1.Code.Gameplay.Combat.Burst
{
    public sealed class HitBatchResult
    {
        public readonly List<HitResult> Hits;

        public HitBatchResult(List<HitResult> hits)
        {
            Hits = hits;
        }
    }
}