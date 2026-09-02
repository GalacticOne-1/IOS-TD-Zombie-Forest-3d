using System;
using Galactic1.Systems.Purchase;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Контракт для всех типов покупок в магазине.
    /// </summary>
    public interface IShopPurchaseRequest
    {
        /// <summary>Можно ли совершить покупку для данного товара</summary>
        bool CanPurchase(IAPConfig config);

        /// <summary>
        /// Запросить покупку
        /// </summary>
        /// <param name="config">Конфигурация товара</param>
        /// <param name="onSuccess">Callback при успешной покупке</param>
        /// <param name="onFail">Callback при ошибке</param>
        void RequestPurchase(
            PurchaseService purchaseService, 
            IAPConfig config, 
            Action onSuccess, 
            Action onFail);
    }

    
}
