using System;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Shop
{
    public class ShopDetailsPanelType3 : ShopDetailsPanelBase
    {
        [Header("UI Elements")] 
        [SerializeField] private GameObject saleIndicator; // включен только если активна опция sale

        [SerializeField] private Transform itemList; // родитель всех слотов LinkedItemSlot

        
        
        public override void Show(
            UIStyleResolver styleResolver,
            IAPConfig config, 
            ShopCardUIBase view, 
            Action<IAPConfig, ShopCardUIBase> onBuy)
        {
            gameObject.SetActive(true);
            currentConfig = config;
            option = config.MainOption;

            // Заголовок панели из базового класса
            if (title != null)
                title.text = config.Header.TitleLid;
            
            // Пока устанавливаю описание
            if (description) 
                description.text = config.Header.DescriptionLid;

            // Настройка спецсостояний панели
            SetupSpecialState(option);

            // Настройка слотов linked items
            SetupLinkedItems(styleResolver, config, view, onBuy);

            transform.GetChild(0).gameObject.RegisterButtonClick(Hide);
            closeButton.RegisterButtonClick(Hide);
        }


        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        private void SetupLinkedItems(
            UIStyleResolver styleResolver,
            IAPConfig config,
            ShopCardUIBase view, 
            Action<IAPConfig, ShopCardUIBase> onBuy)
        {
            if (itemList == null || config.LinkedItems == null) return;

            int slotCount = itemList.childCount;
            int linkedCount = config.LinkedItems.Length;

            bool mainSaleActive = option.sale != null && option.sale.active;
            bool mainBonusActive = option.bonus != null && option.bonus.active;

            // saleIndicator зависит только от mainOption
            if (saleIndicator != null)
                saleIndicator.SetActive(mainSaleActive && !option.sale.onlyPercentLabel);

            for (int i = 0; i < slotCount; i++)
            {
                var _i = i;
                var slotTransform = itemList.GetChild(i);
                var slot = slotTransform.GetComponent<LinkedItemSlot>();
                if (slot == null) continue;

                if (i < linkedCount)
                {
                    var linkedItem = config.LinkedItems[i];
                    var linkedOption = linkedItem.MainOption;

                    // Иконка товара
                    if (linkedItem.RewardItems != null && linkedItem.RewardItems.Length > 0)
                    {
                        var item = GetItem(linkedItem.RewardItems[0].itemId);
                        slot.Bind(item);
                        
                        // === rariry
                        slot.icon.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
                        slot.icon.sprite = item.Header.icon;
                    }
                    slot.icon.gameObject.SetActive(slot.icon.sprite != null);

                    // Количество limited из linkedItem
                    slot.currentCount.text = linkedItem.RewardItems[0].count.ToString();

                    // Старое количество из linkedItem
                    if (slot.oldCountText != null)
                    {
                        slot.oldCountText.text = linkedItem.RewardItems[0].oldCount.ToString();
                        slot.oldCountText.gameObject.SetActive(linkedItem.RewardItems[0].oldCount > 0);
                    }

                    // saleText и bonusText зависят только от mainOption (основного)
                    // if (slot.saleText != null && slot.saleText.transform.parent != null)
                    // {
                    //     slot.saleText.text = mainSaleActive ? $"-{option.sale.percent}%" : "";
                    //     slot.saleText.transform.parent.gameObject.SetActive(mainSaleActive);
                    // }

                    if (slot.bonusText != null && slot.bonusText.transform.parent != null)
                    {
                        slot.bonusText.text = mainBonusActive ? $"x{option.bonus.multiplier}" : "";
                        slot.bonusText.transform.parent.gameObject.SetActive(mainBonusActive);
                    }
                    
                    // Кнопка купить
                    slot.purchaseButton.SetPrice(linkedItem.GetPriceView());
                    slot.purchaseButton.gameObject.RegisterButtonClick(() => onBuy(config.LinkedItems[_i], view));
                    slot.gameObject.SetActive(true);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

    }
}