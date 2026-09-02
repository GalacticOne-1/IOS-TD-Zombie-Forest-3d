namespace Galactic1.UI.Shop
{
    public readonly struct ShopPriceDTO
    {
        public readonly string CurrentPrice;
        public readonly string OldPrice;
        public readonly int DiscountPercent;
        public readonly bool HasDiscount;

        public ShopPriceDTO(
            string currentPrice,
            string oldPrice,
            int discountPercent,
            bool hasDiscount)
        {
            CurrentPrice = currentPrice;
            OldPrice = oldPrice;
            DiscountPercent = discountPercent;
            HasDiscount = hasDiscount;
        }
    }
}