

using System.Collections.Generic;

namespace Galactic1.UI.Shop.Rewards
{
    public class ConvertCurrencyRewardHandler : RewardHandlerBase, IShopRewardHandler
    {
        public ConvertCurrencyRewardHandler(DIContainer container) : base(container) {}

        public ShopRewardType RewardType => ShopRewardType.CurrencyConversion;

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            // if(view != null)
            // {
            //     // PurchaseController.I.PurchaseRegular(
            //     //     config.convertConfig.toAmount, 
            //     //     ShopController.I.currentItem.transform.position,
            //     //     EStat.Soft);
            //     _rootContainer.Resolve<BankResourceService>().AddResource(
            //         EBankResourceType.CurrencySoft,
            //         config.convertConfig.toAmount);
            // }
            // // если карточка потерялась, просто зачисляем награду
            // else
            // {
            //     _rootContainer.Resolve<BankResourceService>().AddResource(
            //         EBankResourceType.CurrencySoft,
            //         config.convertConfig.toAmount);
            // }
        }

        public List<ShopRewardItemData> BuildRewardItems(IAPConfig config) => null;
    }
}