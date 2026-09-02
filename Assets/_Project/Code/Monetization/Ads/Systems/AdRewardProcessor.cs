
using System.Collections.Generic;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Game.Rewards;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Обрабатывает выдачу награды после рекламы.
    /// Может быть расширен серверной верификацией.
    /// </summary>
    public class AdRewardProcessor
    {
        private readonly RewardService rewardService;
        private readonly IAdRewardProvider rewardProvider;

        
        public AdRewardProcessor(
            RewardService rewardService,
            IAdRewardProvider rewardProvider)
        {
            this.rewardService = rewardService;
            this.rewardProvider = rewardProvider;
        }
        
        
        /// <summary>
        /// Выдача награды из конфига
        /// </summary>
        /// <param name="placement"></param>
        public void GrantReward(AdPlacement placement)
        {
            DLog.Alert($"Reward granted {placement}");
            
            var rewardsList = rewardProvider.GetRewards(placement);
            if (rewardsList != null && rewardsList.Count > 0)
            {
                rewardService.GrantRewards(rewardsList, new RewardContext
                {
                    Source = RewardSource.Ad,
                    IsVip = false,
                    EventId = "",
                    ServerMultiplier = 1f
                });
            }
        }
        
        /// <summary>
        /// Выдача динамической награды
        /// </summary>
        /// <param name="rewards"></param>
        /// <param name="context"></param>
        public void GrantReward(List<RewardEntry> rewards, RewardContext context)
        {
            if (rewards == null || rewards.Count == 0)
                return;

            rewardService.GrantRewards(rewards, context);
        }
    }
}