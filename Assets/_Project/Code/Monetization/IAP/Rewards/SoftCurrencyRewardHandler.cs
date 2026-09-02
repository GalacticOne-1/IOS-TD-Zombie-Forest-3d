

using System.Collections.Generic;

namespace Galactic1.UI.Shop.Rewards
{
    public class SoftCurrencyRewardHandler : RewardHandlerBase, IShopRewardHandler
    {
        public SoftCurrencyRewardHandler(DIContainer container) : base(container) {}

        public ShopRewardType RewardType => ShopRewardType.SoftCurrency;

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            
        }
        
        public List<ShopRewardItemData> BuildRewardItems(IAPConfig config) => null;
    }
}