using System;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.UI.Shop
{
    public class ShopCardUIType1 : ShopCardUIOffer
    {
        [Header("Main Image")] 
        public Image mainImage;

        [Header("Small Items")] 
        public Transform itemList;
        public TMP_Text valueText;
        
        
        
        
        public override void Bind(
            UIStyleResolver styleResolver,
            GameStoreWindow shopUI, 
            ShopCardViewModel viewModel,
            Action<IAPConfig, ShopCardUIBase> buyCallback)
        {
            base.Bind(styleResolver, shopUI, viewModel, buyCallback);
            
            if (mainImage != null) 
                mainImage.sprite = viewModel._iapConfig.Header.Icon;
            
            RefreshPrice();
            SetupSpecialState();

            if (!SetupSingleItem(styleResolver))
                SetupSmallItems(styleResolver);

            bCard.RegisterButtonClick(ShowDetails);
            purchaseButton.gameObject.RegisterButtonClick(ShowDetails);
        }


        bool SetupSingleItem(UIStyleResolver styleResolver)
        {
            if (viewModel._iapConfig.RewardItems.Length > 0) 
                return false;

            
            itemList.gameObject.SetActive(false);
            valueText.gameObject.SetActive(true);

            var s = viewModel._iapConfig.RewardType == ShopRewardType.HardCurrency ? "Coins" : "";
            valueText.text = $"{viewModel._iapConfig.RewardSingle.rewardValue} {s}";
            
            return true;
        }
        
        

        private void SetupSmallItems(UIStyleResolver styleResolver)
        {
            if (itemList == null || viewModel._iapConfig.RewardItems == null) return;

            valueText?.gameObject.SetActive(false);
            int slotCount = itemList.childCount;
            int itemCount = viewModel._iapConfig.RewardItems.Length;

            for (int i = 0; i < slotCount; i++)
            {
                var slot = itemList.GetChild(i);
                if (i < itemCount)
                {
                    slot.GetChild(0).gameObject.SetActive(false);
                    var img = slot.GetChild(1).GetComponent<Image>();
                    var countText = slot.GetChild(2).GetComponent<TMP_Text>();

                    var item = GetItem(viewModel._iapConfig.RewardItems[i].itemId);

                    // === rariry
                    img.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
                    img.sprite = item.Header.icon;
                    img.gameObject.SetActive(true);

                    if (viewModel._iapConfig.RewardItems[i].count > 1)
                    {
                        countText.text = viewModel._iapConfig.RewardItems[i].count.ToString();
                        countText.gameObject.SetActive(true);
                    }
                    else
                        countText.gameObject.SetActive(false);

                    if (slot.childCount > 3)
                        slot.GetChild(3).gameObject.SetActive(false);

                    slot.gameObject.SetActive(true);
                }
                else
                {
                    for (int c = 0; c < slot.childCount; c++)
                        slot.GetChild(c).gameObject.SetActive(false);
                    slot.gameObject.SetActive(false);
                }
            }

            if (itemCount > slotCount)
            {
                var lastSlot = itemList.GetChild(slotCount - 1);
                for (int c = 0; c <= 2 && c < lastSlot.childCount; c++)
                    lastSlot.GetChild(c).gameObject.SetActive(false);

                if (lastSlot.childCount > 3)
                {
                    var plusText = lastSlot.GetChild(3).GetComponent<TMP_Text>();
                    plusText.gameObject.SetActive(true);
                    plusText.text = "+" + (itemCount - (slotCount - 1));
                }
            }
        }
    }
}
