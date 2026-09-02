

using System.Collections.Generic;

namespace Galactic1.UI.Shop.Rewards
{
    /// <summary>
    /// Контракт обработчика награды магазина
    /// </summary>
    public interface IShopRewardHandler
    {
        ShopRewardType RewardType { get; }
        void Grant(IAPConfig config, ShopCardUIBase view);
        
        /// <summary>
        /// Данные для отображения на экране наград.
        /// Не выдает награду, только описывает то, что уже было выдано в Grant.
        /// </summary>
        List<ShopRewardItemData> BuildRewardItems(IAPConfig config);
    }
}