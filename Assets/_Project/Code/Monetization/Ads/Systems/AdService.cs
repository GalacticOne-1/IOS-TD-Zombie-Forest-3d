using System;
using System.Threading.Tasks;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Game.Rewards;
using Galactic1.Core;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Главный фасад рекламной системы.
    /// Единственная точка входа для игры.
    /// </summary>
    public class AdService : IGameService
    {
        public readonly AdPolicyEngine Policy;
        public readonly AdEconomyService Economy;
        
        private readonly InterstitialPlacementPolicy interstitialPolicy;
        private readonly AdScheduler scheduler;
        private readonly AdPreloadService preload;
        private readonly AdRewardProcessor rewards;
        private readonly AdCooldownService cooldowns;
        private readonly IAdNetworkAdapter adapter;


        public event Action<AdDecision> OnAdDecisionChanged;


        // Событие, на которое можно подписаться для выдачи награды
        private event Action<AdPlacement> OnGrantReward;

        public void OnGrantRewardEvent(Action<AdPlacement> action)
            => OnGrantReward = action;



        public AdService(
            AdPolicyEngine policy,
            AdScheduler scheduler,
            AdPreloadService preload,
            AdRewardProcessor rewards,
            AdEconomyService economy,
            AdCooldownService cooldowns,
            IAdNetworkAdapter adapter,
            InterstitialPlacementPolicy interstitialPolicy)
        {
            Policy = policy;
            this.interstitialPolicy = interstitialPolicy;
            this.scheduler = scheduler;
            this.preload = preload;
            this.rewards = rewards;
            Economy = economy;
            this.cooldowns = cooldowns;
            this.adapter = adapter;

            // подписки на внутренние изменения
            preload.OnAdLoaded += NotifyDecision;
            cooldowns.OnCooldownFinished += NotifyDecision;
            economy.OnEconomyChanged += NotifyDecision;
        }

        public async Task<AdDecision> TryShowAsync(AdPlacement placement, AdFormat format)
        {
            var decision = Policy.Evaluate();
            if (!decision.Allowed)
                return decision;
            
            if (format == AdFormat.Interstitial)
            {
                if (!interstitialPolicy.CanShow(placement, out var reason))
                    return AdDecision.Deny($"Interstitial placement rule blocked. {reason}");
            }

            if (!adapter.IsReady(format))
                await scheduler.Preload(format);

            if (!adapter.IsReady(format))
                return AdDecision.Deny("Not loaded");

            bool shown = await adapter.ShowAsync(format);
            if (!shown)
                return AdDecision.Deny("Show failed");

            Economy.RegisterShow();
            cooldowns.SetCooldown(10); // пауза между следующим показом рекламы

            NotifyDecision();


            if (format == AdFormat.Rewarded)
            {
                // ✅ уведомляем всех подписчиков о показе рекламы
                if (OnGrantReward != null)
                {
                    OnGrantReward?.Invoke(placement);
                    OnGrantReward = null;
                }
                else
                    rewards.GrantReward(placement);
            }


            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
            return AdDecision.Allow();
        }





        private void NotifyDecision()
        {
            OnAdDecisionChanged?.Invoke(Policy.Evaluate());
        }
    }
}