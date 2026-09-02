
using System;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.UI.Shop
{
    public abstract class ShopDetailsPanelBase : MonoBehaviour
    {
        [Header("Common UI")] 
        public TMP_Text title;
        public TMP_Text description;

        [Header("Special Section (optional)")] 
        public GameObject specialFrame;
        public TMP_Text tSpecH2;
        public TMP_Text tLimit;

        [SerializeField] protected GameObject closeButton;
        [SerializeField] protected PriceButton purchaseButton;
        

        protected IAPConfig currentConfig;
        protected ShopOption option;





        public abstract void Show(
            UIStyleResolver styleResolver,
            IAPConfig config,
            ShopCardUIBase view,
            Action<IAPConfig, ShopCardUIBase> onBuy);
        public abstract void Hide();

        protected void SetupSpecialState(ShopOption option)
        {
            if (option == null) return;

            bool hasSpecial =
                option.hot ||
                (option.sale != null && option.sale.active) ||
                (option.bonus != null && option.bonus.active) ||
                option.limited;

            if (specialFrame != null)
                specialFrame.SetActive(hasSpecial);

            if (!hasSpecial)
            {
                tSpecH2?.gameObject.SetActive(false);
                tLimit?.transform.parent?.gameObject.SetActive(false);
                //tSalePercent?.transform.parent?.gameObject.SetActive(false);
                return;
            }

            Color stateColor = option.limited ? new Color(1f, 0.5f, 0f) : Color.red;

            if (tSpecH2)
            {
                if (option.hot) tSpecH2.text = "HOT";
                else if (option.sale != null && option.sale.active)
                {
                    tSpecH2.text = "SALE";
                    specialFrame.SetActive(!option.sale.onlyPercentLabel);
                }
                else if (option.bonus != null && option.bonus.active) tSpecH2.text = "First Purchase Bonus";
                else if (option.limited) tSpecH2.text = "LIMITED";

                tSpecH2.gameObject.SetActive(true);
                var bg = tSpecH2.transform.parent?.GetComponent<Image>();
                if (bg) bg.color = stateColor;
            }

            if (tLimit)
            {
                if (option.purchaseLimit > 0)
                {
                    tLimit.text = option.purchaseLimit.ToString();
                    tLimit.transform.parent?.gameObject.SetActive(true);
                    var bg = tLimit.transform.parent.GetComponent<Image>();
                    if (bg) bg.color = stateColor;
                }
                else tLimit.transform.parent?.gameObject.SetActive(false);
            }

            // if (tSalePercent)
            // {
            //     if (option.sale != null && option.sale.active)
            //     {
            //         tSalePercent.text = $"-{option.sale.percent}%";
            //         tSalePercent.transform.parent?.gameObject.SetActive(true);
            //         var bg = tSalePercent.transform.parent.GetComponent<Image>();
            //         if (bg) bg.color = stateColor;
            //     }
            //     else tSalePercent.transform.parent?.gameObject.SetActive(false);
            // }
        }

        protected void SetupCommonInfo(IAPConfig config)
        {
            currentConfig = config;
            option = config.MainOption;

            if (title) title.text = config.Header.TitleLid;
            if (description) description.text = config.Header.DescriptionLid;
            //if (priceText && option != null) priceText.text = option.price.ToString();

            SetupSpecialState(option);
        }

        protected void RefreshPrice()
        {
            if(purchaseButton != null)
            {
                var priceDto = currentConfig.GetPriceView();
                purchaseButton.SetPrice(priceDto);
            }
        }


        /// <summary>
        /// Для получения предмета
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemConfig GetItem(ItemId id) => GameContent.Items.Get(id);
    }
}
