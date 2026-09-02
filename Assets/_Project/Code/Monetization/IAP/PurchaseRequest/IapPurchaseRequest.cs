using System;
using Galactic1.Systems.Purchase;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Покупка через IAP
    /// </summary>
    public class IapPurchaseRequest : IShopPurchaseRequest
    {
        private DIContainer _rootContainer;
        
        public IapPurchaseRequest(DIContainer container)
        {
            _rootContainer = container;
        }
        
        public bool CanPurchase(IAPConfig config)
        {
            return config.UseIAP;
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

            purchaseService.Buy(config.ProductId);
            onSuccess?.Invoke(); // или через событие PurchaseService
        }
    }
}