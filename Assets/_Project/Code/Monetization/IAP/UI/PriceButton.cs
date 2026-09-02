using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.UI.Shop
{
    public class PriceButton : ButtonUIRegular
    {
        [SerializeField] private TMP_Text tPrice;
        [SerializeField] private TMP_Text tOldPrice;
        [SerializeField] private TMP_Text tDiscount;

        
        
        public void SetPrice(ShopPriceDTO price)
        {
            tPrice.text = price.CurrentPrice;

            bool hasDiscount = price.HasDiscount;

            if (tOldPrice != null)
            {
                tOldPrice.gameObject.SetActive(hasDiscount);
                if (hasDiscount)
                    tOldPrice.text = price.OldPrice;
            }

            if (tDiscount != null)
            {
                tDiscount.transform.parent.gameObject.SetActive(hasDiscount);

                if (hasDiscount)
                    tDiscount.text = $"-{price.DiscountPercent}%";
            }
        }
    }
}