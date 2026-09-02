
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Window;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Galactic1.UI.Shop
{
    [CreateAssetMenu(fileName = "IAPConfig", menuName = "Game Configs/Monetisation/New IAP Config")]
    public class IAPConfig : WindowCardInitialStateConfigs
    {

        // Настройки для панели магазина
        
        [field: SerializeField] public ShopCategory Category { get; private set; }
        [field: SerializeField] public EViewType ViewType { get; private set; } = EViewType.Single1;
        // карточка несет одну покупку или список разных покупок
        
        
        [field: Space(20)]
        [field: Header("Purchase")]
        [field: SerializeField] public bool UseIAP { get; private set; } = true;
        [field: SerializeField] public string ProductId { get; private set; }
        [field: SerializeField] public ProductType ProductType { get; private set; }
        [field: SerializeField] public bool OneTimePurchase { get; private set; }
        
        
        [field:Header("Главная опция")]
        [field: SerializeField] public ShopOption MainOption { get; private set; }     // опции покупки
        
        
        // *** REWARD
        [field:Header("Что игрок получает за покупку")]
        [field: SerializeField] public bool DisableAd { get; private set; } = true;
        
        [field: SerializeField] public ShopRewardType RewardType { get; private set; }
        
        [field: Tooltip("Если награда за покупку ресурс из банка")]
        [field: SerializeField] public ShopSingleReward RewardSingle { get; private set; }
        
        [field: Tooltip("Если награда за покупку предмет/ы")]
        [field: SerializeField] public SmallItemsReward[] RewardItems { get; private set; }
        // если конфиг используется в другом конфиге, то работает только один товар RewardItems[0] !!!

        

        [field:Header("Type2 BasicSettings")]
        [field: SerializeField] public IAPConfig[] LinkedItems { get; private set; }  
        // Для Type2 — конфиги, которые будут отображены в itemList



        #region LIVE PRICE
        
        
        public bool HasSale =>
            MainOption.sale != null && MainOption.sale.active;

        public int SalePercent =>
            HasSale ? MainOption.sale.percent : 0;
        
        public string PriceL { get; private set; }
        public decimal Price { get; private set; }
        public string CurrencyCode { get; private set; }

        public void SetPrice(string localizedPrice, decimal price, string currencyCode)
        {
            PriceL = localizedPrice;
            Price = price;
            CurrencyCode = currencyCode;
        }
        
        /// <summary>
        /// Вернет рабочую цену
        /// </summary>
        /// <returns></returns>
        public ShopPriceDTO GetPriceView()
        {
            var linkedItem = LinkedItems.Length > 0 ? LinkedItems[0] : null;
            decimal price = linkedItem?.Price ?? Price;
            string priceL = linkedItem?.PriceL ?? PriceL;
            int salePercent = linkedItem?.SalePercent ?? SalePercent;
            
            if (!HasSale)
            {
                return new ShopPriceDTO(
                    priceL,
                    null,
                    0,
                    false);
            }

            int percent = Mathf.Clamp(salePercent, 1, 99);

            decimal oldPrice = price / (1m - percent / 100m);

            return new ShopPriceDTO(
                priceL,
                $"{oldPrice.ToString("0")} {CurrencyCode}" ,
                percent,
                true);
        }
        

        #endregion

        
    }
    
    
    [System.Serializable]
    public class ShopSingleReward
    {
        public int rewardValue;
    }
    
    [System.Serializable]
    public class SmallItemsReward          // предмет для выдачи
    {
        public ItemId itemId; 
        public int count;           // текущее количество
        public int oldCount;        // старое количество
    }

    [System.Serializable]
    public class ShopOption
    {
        public int purchaseLimit;        // количество покупок
    
        [Space]
        public bool limited;
        public bool hot;
        public SaleData sale;
        public FirstPurchaseBonus bonus;
    }


    [System.Serializable]
    public class SaleData
    {
        public bool active;
        [Tooltip("Отключaет рамку")]
        public bool onlyPercentLabel;
        public int percent;
    }

    [System.Serializable]
    public class FirstPurchaseBonus
    {
        public bool active;
        public int multiplier; // x2, x3, x4
    }
}