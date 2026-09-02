using UnityEngine;
using System.Collections.Generic;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Game.Rewards
{
    [CreateAssetMenu(
        fileName = "AdRewardConfig",
        menuName = "Game Configs/Reward System/Reward Config"
    )]
    public class AdRewardsConfig : ScriptableObject
    {
        [field: SerializeField] public List<AdPlacementRewardGroup> RewardsAd {get; private set;}

        
        public Dictionary<AdPlacement, List<RewardEntry>> BuildLookupAd()
        {
            var dict = new Dictionary<AdPlacement, List<RewardEntry>>();
            foreach (var r in RewardsAd)
                dict[r.placement] = r.rewards;
            return dict;
        }
        
    }
}