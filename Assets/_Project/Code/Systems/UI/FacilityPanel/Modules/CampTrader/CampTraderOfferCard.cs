using System;
using System.Collections;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Ads;
using Galactic1.Code.UI.Common.Effects;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Core.Notifications;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Trader;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает предложение трейдера: предмет, durability, amount, кнопки покупки.
    /// </summary>
    public class CampTraderOfferCard : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image icon;

        [Header("Stats")]
        [SerializeField] private TMP_Text durabilityText;
        [SerializeField] private Image durabilityBar;
        [SerializeField] private TMP_Text amountText;

        [Header("Actions")]
        [SerializeField] private GameObject premiumBuyButton;
        [SerializeField] private GameObject adsBuyButton;

        private UIFadeComponent fadeComponent;

        private string _offerId;
        private CampTraderSceneAdapter _adapter;
        
        private ItemConfig item;
        private int durability;
        private TooltipInputHandler inputHandler;


        public void Bind(
            CampTraderOfferDTO offer,
            CampTraderSceneAdapter adapter)
        {
            _adapter = adapter;
            _offerId = offer.Id;
            
            item = offer.Item;
            durability = offer.Durability;

            // эффект появления карточки
            fadeComponent = GetComponent<UIFadeComponent>();
            fadeComponent.Setup();
            fadeComponent.SetInstant(0f); // карточка изначально скрыта
            gameObject.GetChild(0).SetActive(false);

            // === Item
            nameText.text = offer.ItemNameLid;
            icon.sprite = offer.Icon;

            // === Durability (не у всех предметов есть)
            durabilityText.gameObject.SetActive(offer.UsesDurability);
            durabilityBar.transform.parent.gameObject.SetActive(offer.UsesDurability);
            if (offer.UsesDurability)
            {
                durabilityText.text = $"{(int)(offer.Durability01 * 100)}%";
                durabilityBar.fillAmount = offer.Durability01;
            }

            // === Amount
            amountText.text = $"x{offer.Amount}";

            // === Premium buy
            premiumBuyButton.GetChild(0).CMP_Text().text = offer.Cost.ToString();

            premiumBuyButton.RegisterButtonClick(() =>
            {
                var data = new ConfirmPaidPopupData(
                    "Confirm Purchase",
                    "Are you sure you want to buy this item?",
                    "Yes",
                    $"{offer.Cost}",
                    onOk: () =>
                    {
                        var response = _adapter.TryBuy(_offerId, PurchaseType.PremiumCurrency);
                        if (response.result.Success)
                            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(
                                finishBuy(response.finishAction, offer.ItemNameLid, premiumBuyButton));
                    },
                    onClose: () => {}
                );

                ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPaidPopup, data);
            });

            // === Ads buy
            adsBuyButton.RegisterButtonClick(() =>
            {
                if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService)
                {
                    ServiceLocator.Current.Get<AdService>().OnGrantRewardEvent(p =>
                    {
                        var response = _adapter.TryBuy(_offerId, PurchaseType.Ads);
                        if (response.result.Success)
                            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(
                                finishBuy(response.finishAction, offer.ItemNameLid, adsBuyButton));
                    });
                }
            });

            // добавляем появление
            fadeComponent.FadeIn(true, () => gameObject.GetChild(0).SetActive(true));
            
            
            // === подсказка
            inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }
        
        private void HandleHoldStart(RectTransform anchor)
            => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                TooltipType.Loot,
                gameObject.CMP_RectTr(),
                item,
                durability);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();


        IEnumerator finishBuy(
            Action claim,
            string itemName,
            GameObject button)
        {
            button.SetActive(false);

            // bool finished = false;
            // fadeComponent.FadeOut(() => finished = true);
            //
            // yield return new WaitUntil(() => finished);

            if (claim == null)
                yield break;

            claim();

            ServiceLocator.Current.Get<INotificationService>().Push(
                new NotificationRequest(
                    "trader_purchase",
                    $"{itemName} added to inbox",
                    NotificationPriority.Normal,
                    NotificationChannel.Toast,
                    NotificationStyleCategory.Default,
                    new NotificationMessageConfig.NotificationStyle()));
        }
    }
}
