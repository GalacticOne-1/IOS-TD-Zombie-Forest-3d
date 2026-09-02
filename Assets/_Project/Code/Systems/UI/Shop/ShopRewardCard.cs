using System;
using Galactic1.Code.UI.Common.Effects;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.UI.Shop.Rewards
{
    /// <summary>
    /// Карточка одной награды. Только визуал, без корутин.
    /// </summary>
    public class ShopRewardCard : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;


        private UIStyleResolver styleResolver;
        private TooltipInputHandler inputHandler;
        private UIFadeComponent _fade;
        private IUIFlashEffect _flash;
        private ItemConfig item;
        
        

        private void Awake()
        {
            _flash = GetComponent<IUIFlashEffect>();
            _fade = GetComponent<UIFadeComponent>();
            styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();
        }

        public void Bind(ShopRewardItemData data)
        {
            // подсказка только для предметов
            item = data.ItemConfig;

            if (item != null)
                icon.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
            
            icon.sprite = data.ItemConfig?.Header.icon ?? data.CurrencyConfig?.Header.icon;
            amount.text = $"{data.Amount}";
            
            Hide();
            gameObject.SetActive(true);
            
            
            // === подсказка
            inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }

        public void Show(Action onFinished = null)
        {
            _fade.SetInstant(0f);
            _fade.FadeIn(false, () =>
            {
                // 1. Flash shader
                _flash.Play(() =>
                {
                    // 2. Enable root AFTER flash
                    root.SetActive(true);
                    onFinished();
                });
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _fade.Setup();
            _fade.SetInstant(0f);
            root.SetActive(false);
        }


        private void HandleHoldStart(RectTransform anchor)
        {
            if(item != null)
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