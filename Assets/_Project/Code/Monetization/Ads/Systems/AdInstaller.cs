
using Galactic1.Code.Game.Rewards;
using Galactic1.Code.Systems.Daily;
using Galactic1.Configs;
using Galactic1.Core;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Создаёт и связывает все рекламные подсистемы.
    /// Вызывается при старте игры (GameSessionManager).
    /// </summary>
    public static class AdInstaller
    {
        public static AdService Create(
            DIContainer container, 
            Coroutines routine,
            IAdNetworkAdapter adapter, 
            AdPreloadService preload)
        {
            var cooldowns = new AdCooldownService(routine);
            var economy = new AdEconomyService(container);
            container.Resolve<TimeBoundaryService>().RegisterRule(economy);
            
            var policy = new AdPolicyEngine(cooldowns, economy);
            var scheduler = new AdScheduler(adapter);
            
            // === Добавляем правила для interstitial 
            var interstitialPolicy = new InterstitialPlacementPolicy();
            interstitialPolicy.AddRule(new PostRaidRule());
            interstitialPolicy.AddRule(new MinSessionTimeRule(120f)); // минимум 2 минуты сессии
            
            // === для наград
            var rewardProvider = container.Resolve<IAdRewardProvider>();
            var rewardService = container.Resolve<RewardService>();
            var rewards = new AdRewardProcessor(rewardService, rewardProvider);
            

            return new AdService(
                policy, 
                scheduler, 
                preload, 
                rewards,
                economy, 
                cooldowns, 
                adapter,
                interstitialPolicy);
        }
    }
}