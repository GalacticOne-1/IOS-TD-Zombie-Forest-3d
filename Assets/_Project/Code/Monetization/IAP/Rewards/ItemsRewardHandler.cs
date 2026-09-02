
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Game.Meta.Items;

namespace Galactic1.UI.Shop.Rewards
{
    public class ItemsRewardHandler : RewardHandlerBase, IShopRewardHandler
    {
        public ItemsRewardHandler(DIContainer container) : base(container){}

        public ShopRewardType RewardType => ShopRewardType.ItemPack;

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            var registry = GameContent.Items;
            var inboxService = ServiceLocator.Current.Get<InboxService>();
            ItemConfig item;
            SmallItemsReward reward;

            var l = config.RewardItems.Length;
            for (int i = 0; i < l; i++)
            {
                reward = config.RewardItems[i];
                item = registry.Get(reward.itemId);

                inboxService.AddReward(
                    new InventorySlotRuntime(
                        item,
                        reward.count,
                        item.Physical.maxDurability,
                        0));
            }
        }
        
        public List<ShopRewardItemData> BuildRewardItems(IAPConfig config)
        {
            var registry = GameContent.Items;
            var result = new List<ShopRewardItemData>(config.RewardItems.Length);
 
            var l = config.RewardItems.Length;
            for (int i = 0; i < l; i++)
            {
                var reward = config.RewardItems[i];
                var item = registry.Get(reward.itemId);

                result.Add(new(null, item, reward.count));
            }
 
            return result;
        }
    }
}