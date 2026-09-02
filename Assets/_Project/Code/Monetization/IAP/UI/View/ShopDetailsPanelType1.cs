using System;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.UI.Shop
{
    public class ShopDetailsPanelType1 : ShopDetailsPanelBase
    {
        [SerializeField] private Image mainImage;
        [SerializeField] private Transform smallItemList;
        
        

        public override void Show(
            UIStyleResolver styleResolver,
            IAPConfig config, 
            ShopCardUIBase view, 
            Action<IAPConfig, ShopCardUIBase> onBuy)
        {
            gameObject.SetActive(true);

            SetupCommonInfo(config);

            if (mainImage)
                mainImage.sprite = config.Header.Icon;
            
            SetupSmallItems(styleResolver, config);
            RefreshPrice();
            
            purchaseButton.gameObject.RegisterButtonClick(() => onBuy(config, view));

            transform.GetChild(0).gameObject.RegisterButtonClick(Hide);
            closeButton.RegisterButtonClick(Hide);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        private void SetupSmallItems(UIStyleResolver styleResolver, IAPConfig config)
        {
            if (smallItemList == null || config.RewardItems == null) 
                return;

            int slotCount = smallItemList.childCount;
            int itemCount = config.RewardItems.Length;

            for (int i = 0; i < slotCount; i++)
            {
                var slot = smallItemList.GetChild(i);
                if (i < itemCount)
                {
                    var item = GetItem(config.RewardItems[i].itemId);
                    slot.GetComponent<ItemCardBinder>().Bind(item);
                    
                    slot.GetChild(0).gameObject.SetActive(false);
                    var img = slot.GetChild(1).GetComponent<Image>();
                    var countText = slot.GetChild(2).GetComponent<TMP_Text>();
                    
                    // === rariry
                    img.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
                    img.sprite = item.Header.icon;
                    img.gameObject.SetActive(true);

                    countText.gameObject.SetActive(config.RewardItems[i].count > 1);
                    if (config.RewardItems[i].count > 1)
                        countText.text = config.RewardItems[i].count.ToString();

                    if (slot.childCount > 3)
                    {
                        slot.GetChild(3).gameObject.SetActive(false);
                        slot.GetComponent<Image>().raycastTarget = false;
                    }

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
                var lastSlot = smallItemList.GetChild(slotCount - 1);
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