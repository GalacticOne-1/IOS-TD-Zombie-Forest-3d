
using Galactic1.Game.Meta.Economy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.UI.Shop.Rewards
{
    /// <summary>
    /// Данные для отображения одной награды на экране наград.
    /// Только данные, без поведения.
    /// </summary>
    public struct ShopRewardItemData
    {
        public CurrencyConfig CurrencyConfig { get; }
        public ItemConfig ItemConfig { get; }
        public int Amount { get; }

        public ShopRewardItemData(
            CurrencyConfig currencyConfig, 
            ItemConfig itemConfig, 
            int amount)
        {
            CurrencyConfig = currencyConfig;
            ItemConfig = itemConfig;
            Amount = amount;
        }
    }
}