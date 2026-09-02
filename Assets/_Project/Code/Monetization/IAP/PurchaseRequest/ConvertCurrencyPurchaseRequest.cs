using System;
using Galactic1.Systems.Purchase;

namespace Galactic1.UI.Shop
{
    public class ConvertCurrencyPurchaseRequest : IShopPurchaseRequest
    {
        private DIContainer _rootContainer;
        
        public ConvertCurrencyPurchaseRequest(DIContainer container)
        {
            _rootContainer = container;
        }
        
        public bool CanPurchase(IAPConfig config)
        {
            if (config.RewardType != ShopRewardType.CurrencyConversion)
                return false;

            //return DevManager.I.game.not_use_money || HUBStat.HaveGems(config.convertConfig.fromAmount);
            return false;
        }

        public void RequestPurchase(
            PurchaseService purchaseService,
            IAPConfig config, 
            Action onSuccess, 
            Action onFail)
        {
            if (!CanPurchase(config))
            {
                onFail?.Invoke();
                return;
            }

            // var context = new PurchaseContext
            // {
            //     title = "Convert Currency",
            //     description = config.description,
            //     price = config.convertConfig.fromAmount,
            //     onConfirm = () =>
            //     {
            //         ShopController.I.CompleteHardPurchase(config);
            //         onSuccess?.Invoke();
            //     }
            // };
            //
            // ShopConfirmPanel.I.Open(context);
        }
    }
}