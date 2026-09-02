using System;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.UI.Shop
{
    public class ShopCardUITypeMulty1 : ShopCardUIOffer
    {
        [Header("Item Slots (4)")] 
        [SerializeField] private Transform itemList;

        
        public override void Bind(
            UIStyleResolver styleResolver,
            GameStoreWindow shopUI, 
            ShopCardViewModel viewModel,
            Action<IAPConfig, ShopCardUIBase> buyCallback)
        {
            base.Bind(styleResolver, shopUI, viewModel, buyCallback);
            
            RefreshPrice();
            SetupSpecialState();
            SetupLinkedItems(styleResolver);

            bCard.RegisterButtonClick(ShowDetails);
            purchaseButton.gameObject.RegisterButtonClick(ShowDetails);
        }

        private void SetupLinkedItems(UIStyleResolver styleResolver)
        {
            if (itemList == null || viewModel._iapConfig.LinkedItems == null) return;

            int slotCount = itemList.childCount;
            int linkedCount = viewModel._iapConfig.LinkedItems.Length;

            for (int i = 0; i < slotCount; i++)
            {
                var slot = itemList.GetChild(i);

                if (i < linkedCount)
                {
                    var linked = viewModel._iapConfig.LinkedItems[i];
                    var img = slot.GetChild(0).GetComponent<Image>();

                    if (linked.RewardItems != null && linked.RewardItems.Length > 0)
                    {
                        var item = GetItem(linked.RewardItems[0].itemId);
                        // === rariry
                        img.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
                        img.sprite = item.Header.icon;
                    }
                    else
                        img.sprite = null;

                    img.gameObject.SetActive(img.sprite != null);

                    for (int c = 1; c < slot.childCount; c++)
                        slot.GetChild(c).gameObject.SetActive(false);

                    slot.gameObject.SetActive(true);
                }
                else
                {
                    for (int c = 0; c < slot.childCount; c++)
                        slot.GetChild(c).gameObject.SetActive(false);
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }
}
