using System;
using Galactic1.Systems.Purchase;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Внутренняя покупка без IAP
    /// </summary>
    public class InternalPurchaseRequest : IShopPurchaseRequest
    {
        private DIContainer _rootContainer;
        
        public InternalPurchaseRequest(DIContainer container)
        {
            _rootContainer = container;
        }

        public bool CanPurchase(IAPConfig config)
        {
            return !config.UseIAP;
        }

        public void RequestPurchase(
            PurchaseService purchaseService,
            IAPConfig config, 
            Action onSuccess, 
            Action onFail)
        {
            // Прямая выдача награды
            _rootContainer.Resolve<ShopController>().CompleteHardPurchase(config);
            onSuccess?.Invoke();
        }
    }
}