

namespace Galactic1.UI.Shop
{
    public static class ShopPurchaseRequestFactory
    {
        public static IShopPurchaseRequest Create(IAPConfig config, DIContainer container)
        {
            if (config.UseIAP)
                return new IapPurchaseRequest(container);

            if (config.RewardType == ShopRewardType.CurrencyConversion)
                return new ConvertCurrencyPurchaseRequest(container);

            return new InternalPurchaseRequest(container);
        }
    }

}