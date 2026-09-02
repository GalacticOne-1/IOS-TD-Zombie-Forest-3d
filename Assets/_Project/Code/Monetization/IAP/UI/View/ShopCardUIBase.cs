using System;
using Galactic1.UI.Core;
using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// View одной карточки магазина.
    /// Отвечает только за UI и пользовательский ввод.
    /// </summary>
    public abstract class ShopCardUIBase : MonoBehaviour
    {
        [Header("Common UI")] 
        public TMP_Text itemName;
        public TMP_Text description;

        [Header("Special Section")] 
        public GameObject specialFrame; // рамка для спецсостояний
        public TMP_Text tSpecH2; // текст состояния (HOT, SALE, etc.)
        public TMP_Text tLimit; // текст с количеством limited
        public TMP_Text tSalePercent; // процент скидки
        public TMP_Text tFirstPurchaseBonusText;

        [Header("Кнопки")]
        [SerializeField] protected GameObject bCard;
        [SerializeField] protected GameObject bFree;
        [SerializeField] protected PriceButton purchaseButton;
        
        
        protected GameStoreWindow shopUI;
        protected ShopCardViewModel viewModel;
        protected ShopOption option;
        protected UIStyleResolver _styleResolver;
        
        private Action<IAPConfig, ShopCardUIBase> onBuy;
        
        
        public virtual void Bind(
            UIStyleResolver styleResolver,
            GameStoreWindow shopUI, 
            ShopCardViewModel viewModel,
            Action<IAPConfig, ShopCardUIBase> buyCallback)
        {
            this.shopUI = shopUI;
            this.viewModel = viewModel;
            onBuy = buyCallback;
            _styleResolver = styleResolver;
            
            //viewModel.Limit.Subscribe(_ => tLimit.text = _.ToString());
            
            option = viewModel._iapConfig.MainOption;
            itemName.text = viewModel._iapConfig.Header.TitleLid;
            description.text = viewModel._iapConfig.Header.DescriptionLid;
        }
        
        //public abstract void Setup(ShopItemSO itemSO, ShopUI parentUI);

        protected void SetupSpecialState()
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
                tSalePercent?.transform.parent?.gameObject.SetActive(false);
                tFirstPurchaseBonusText?.transform.parent?.gameObject.SetActive(false);
                return;
            }

            Color stateColor = option.limited ? new Color(1f, 0.5f, 0f) : Color.red;

            // рамка
            var frameImage = specialFrame?.GetComponent<Image>();
            if (frameImage != null) frameImage.color = stateColor;

            // tSpecH2 и родитель
            if (tSpecH2 != null && tSpecH2.transform.parent != null)
            {
                tSpecH2.gameObject.SetActive(true);
                var parentImage = tSpecH2.transform.parent.GetComponent<Image>();
                if (parentImage != null) parentImage.color = stateColor;

                if (option.hot) tSpecH2.text = "HOT";
                else if (option.sale != null && option.sale.active)
                {
                    tSpecH2.text = "SALE";
                    specialFrame.SetActive(!option.sale.onlyPercentLabel);
                }
                else if (option.bonus != null && option.bonus.active) tSpecH2.text = "First Purchase Bonus";
                else if (option.limited) tSpecH2.text = "LIMITED";
            }

            // tLimit и родитель
            if (tLimit != null && tLimit.transform.parent != null)
            {
                var parentImage = tLimit.transform.parent.GetComponent<Image>();
                if (parentImage != null) parentImage.color = stateColor;

                if (option.purchaseLimit > 0)
                {
                    tLimit.text = option.purchaseLimit.ToString();
                    tLimit.transform.parent.gameObject.SetActive(true);
                }
                else
                    tLimit.transform.parent.gameObject.SetActive(false);
            }

            // tSalePercent
            if (tSalePercent != null && tSalePercent.transform.parent != null)
            {
                var parentImage = tSalePercent.transform.parent.GetComponent<Image>();
                if (parentImage != null) parentImage.color = stateColor;

                if (option.sale != null && option.sale.active)
                {
                    tSalePercent.text = $"-{option.sale.percent}%";
                    tSalePercent.transform.parent.gameObject.SetActive(true);
                }
                else
                    tSalePercent.transform.parent.gameObject.SetActive(false);
            }

            // tFirstPurchaseBonusText
            if (tFirstPurchaseBonusText != null && tFirstPurchaseBonusText.transform.parent != null)
            {
                var parentImage = tFirstPurchaseBonusText.transform.parent.GetComponent<Image>();
                if (parentImage != null) parentImage.color = stateColor;

                if (option.bonus != null && option.bonus.active)
                {
                    tFirstPurchaseBonusText.text = $"x{option.bonus.multiplier}";
                    tFirstPurchaseBonusText.transform.parent.gameObject.SetActive(true);
                }
                else
                    tFirstPurchaseBonusText.transform.parent.gameObject.SetActive(false);
            }
        }
        
        protected void RefreshPrice()
        {
            if(purchaseButton != null)
            {
                var priceDto = viewModel._iapConfig.GetPriceView();
                purchaseButton.SetPrice(priceDto);
            }
        }

        protected virtual void ShowDetails()
        {
            if (shopUI != null)
                shopUI.OpenDetailsPanel(_styleResolver, viewModel._iapConfig, this, onBuy);
        }
    }
}