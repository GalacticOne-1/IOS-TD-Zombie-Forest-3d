using System.Collections.Generic;
using Galactic1.Code.Core.Ads;
using UnityEngine;

namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Сервис, отвечающий за бонусы за просмотр рекламы
    /// </summary>
    public class AdRewardConfigProvider : IAdRewardProvider
    {
        private readonly Dictionary<AdPlacement, List<RewardEntry>> mapRewards;

        public AdRewardConfigProvider(AdRewardsConfig config)
        {
            mapRewards = config.BuildLookupAd();
        }


        #region Ad

        /// <summary>
        /// Возвращает мультипликатор бонуса за рекламу
        /// </summary>
        public float GetAdMultiplier(AdPlacement placement)
        {
            if (mapRewards.TryGetValue(placement, out var rewards))
                return rewards[0].multiplier;

            return 1f; // бонус не активен
        }
        
        /// <summary>
        /// Применяет бонус за рекламу к базовому количеству
        /// </summary>
        public int ApplyAdMultiplier(AdPlacement placement, int baseAmount)
        {
            float multiplier = GetAdMultiplier(placement);
            return Mathf.CeilToInt(baseAmount * multiplier);
        }
        

        #endregion
        
        
        public IReadOnlyList<RewardEntry> GetRewards(AdPlacement placement)
        {
            return mapRewards.TryGetValue(placement, out var rewards)
                ? rewards
                : System.Array.Empty<RewardEntry>();
        }

        
    }
}