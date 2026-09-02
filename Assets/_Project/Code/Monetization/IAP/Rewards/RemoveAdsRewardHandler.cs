

using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Core;

namespace Galactic1.UI.Shop.Rewards
{
    public class RemoveAdsRewardHandler : RewardHandlerBase, IShopRewardHandler
    {
        public RemoveAdsRewardHandler(DIContainer container) : base(container){}

        public ShopRewardType RewardType => ShopRewardType.RemoveAds;

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            StateWriter.Write(_rootContainer.Resolve<IGameStateProvider>().GameStateProxy.ADState,
                (ref CGameStateAD ad) =>
                {
                    ad.ShowAutoAds = false;
                });
        }
        
        public List<ShopRewardItemData> BuildRewardItems(IAPConfig config) => null;
    }
}