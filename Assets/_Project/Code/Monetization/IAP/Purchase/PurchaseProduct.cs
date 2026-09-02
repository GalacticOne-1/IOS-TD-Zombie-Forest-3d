using UnityEngine.Purchasing;

namespace Galactic1.Systems.Purchase
{
    /// <summary>
    /// Описание продукта для PurchaseService
    /// </summary>
    public struct PurchaseProduct
    {
        public string productId;
        public ProductType type;
    }
}