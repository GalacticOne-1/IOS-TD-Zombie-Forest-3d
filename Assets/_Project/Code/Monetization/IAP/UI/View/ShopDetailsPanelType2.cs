using System;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.UI.Shop
{
    public class ShopDetailsPanelType2 : ShopDetailsPanelBase
    {
        [SerializeField] private Image mainImage;
        [SerializeField] private TMP_Text rewardValueText;

        private TooltipInputHandler inputHandler;
        private ItemConfig item;
        
        public override void Show(
            UIStyleResolver styleResolver,
            IAPConfig config, 
            ShopCardUIBase view, 
            Action<IAPConfig, ShopCardUIBase> onBuy)
        {
            
            item = config.RewardItems.Length > 0 ? GetItem(config.RewardItems[0].itemId) : null;
            
            gameObject.SetActive(true);

            SetupCommonInfo(config);

            rewardValueText.text = $"{config.RewardSingle.rewardValue}";

            if (mainImage) 
                mainImage.sprite = config.Header.Icon;

            RefreshPrice();
            purchaseButton.gameObject.RegisterButtonClick(() => onBuy(config, view));

            transform.GetChild(0).gameObject.RegisterButtonClick(Hide);
            closeButton.RegisterButtonClick(Hide);
            
            
            // === подсказка
            inputHandler = GetComponentInChildren<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleHoldStart(RectTransform anchor)
        {
            if (item != null)
            {
                ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                    TooltipType.Loot,
                    gameObject.CMP_RectTr(),
                    item,
                    item.Physical.maxDurability);
            }
        }

        private void HandleHoldEnd()
        {
            if (item != null)
                ServiceLocator.Current.Get<TooltipController>().Hide();
        }
    }
}