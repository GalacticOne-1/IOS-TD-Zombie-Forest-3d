
using System.Collections.Generic;
using Galactic1.Code.Game.Rewards.Modifiers;
using Galactic1.Code.Systems.Economy;

namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Центральный сервис применения наград.
    /// Единственное место, где меняется экономика.
    /// </summary>
    public class RewardService : IGameService
    {
        private readonly IEconomyService _economyService;
        private readonly RewardPipeline _pipeline;
        
        

        public RewardService(DIContainer container)
        {
            _economyService = container.Resolve<IEconomyService>();
            
            _pipeline = new RewardPipeline(new IRewardModifier[]
            {
                new AdRewardModifier(),
                new VipRewardModifier()
            });
            
        }


       

        public void GrantRewards(IEnumerable<RewardEntry> rewards, RewardContext ctx)
        {
            foreach (var reward in rewards)
            {
                int finalAmount = _pipeline.ResolveAmount(reward, ctx);
                ApplyReward(reward, finalAmount);
            }
        }
        
        private void ApplyReward(RewardEntry reward, int amount)
        {
            switch (reward.type)
            {
                case RewardType.CurrencyHard:
                    _economyService.Add(EBankResourceType.CurrencyPremium, amount);
                    break;
                
                case RewardType.CurrencySoft:
                    _economyService.Add(EBankResourceType.CurrencySoft, amount);
                    break;

                case RewardType.Item:
                    
                    break;
            }
            
            DLog.Alert($"Granted reward: {reward.type} / {amount}");
        }
    }
}