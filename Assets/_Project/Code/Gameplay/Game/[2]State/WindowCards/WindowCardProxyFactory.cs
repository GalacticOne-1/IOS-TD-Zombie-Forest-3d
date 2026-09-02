using System;
using Galactic1.UI.Shop;

namespace Galactic1.Window
{
    public class WindowCardProxyFactory
    {
        public static WindowCardProxy CreateCard(WindowCardData cardData)
        {
            switch (cardData.Type)
            {
                case EWindowCardType.IAP:
                    return new ShopCardProxy(cardData as IAPCardData);
                
                
                
                default:
                    throw new Exception("Unsupported card type: " + cardData.Type);
            }
        }
    }
}