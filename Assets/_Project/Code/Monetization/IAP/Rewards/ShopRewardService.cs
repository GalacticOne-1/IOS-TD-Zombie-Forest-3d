using System.Collections.Generic;
using Galactic1.Code.UI.Stations;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Shop.Rewards
{
    /// <summary>
    /// Центральный сервис выдачи наград магазина
    /// </summary>
    public class ShopRewardService
    {
        private readonly Dictionary<ShopRewardType, IShopRewardHandler> handlers = new();
        private readonly DIContainer _rootContainer;

        public ShopRewardService(DIContainer container)
        {
            _rootContainer = container;

            Register(new HardCurrencyRewardHandler(container));
            Register(new SoftCurrencyRewardHandler(container));
            Register(new RemoveAdsRewardHandler(container));
            Register(new ConvertCurrencyRewardHandler(container));
            Register(new ItemsRewardHandler(container));
        }
        
        

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            if (!handlers.TryGetValue(config.RewardType, out var handler))
            {
                Debug.LogError($"No reward handler for {config.RewardType}");
                return;
            }

            handler.Grant(config, view);

            var rewards = handler.BuildRewardItems(config);
            if (rewards.Count == 0)
                return;

            ServiceLocator.Current.Get<UIManager>()
                .OpenScreen(UIScreenId.PurchaseRewardScreen, null,
                    _ => _.GetComponent<ShopRewardScreen>().OnShow(rewards));
        }

        private void Register(IShopRewardHandler handler)
        {
            handlers[handler.RewardType] = handler;
        }
    }
}