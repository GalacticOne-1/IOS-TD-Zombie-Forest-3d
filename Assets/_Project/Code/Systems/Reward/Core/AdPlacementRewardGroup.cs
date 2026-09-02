using System.Collections.Generic;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Game.Rewards
{
    [System.Serializable]
    public class AdPlacementRewardGroup
    {
        public AdPlacement placement;
        public List<RewardEntry> rewards;
    }
}