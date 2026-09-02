using System;
using System.Collections;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Ads;
using Galactic1.Code.UI.Common.Effects;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Core.Notifications;
using Galactic1.Game.Runtime.Recruitment;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает кандидата, экипировку, статы и кнопку найма.
    /// </summary>
    public class TavernOfferCard : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private RawImage modelTexture;

        [Header("Equipment")]
        [SerializeField] private TavernOfferEquipmentSlotView weaponSlot;
        [SerializeField] private Transform gearSlots;

        [Header("Stats")]
        [SerializeField] private Transform statRoot;

        [Header("Actions")]
        [SerializeField] private GameObject freeHireButton;
        [SerializeField] private GameObject premiumHireButton;
        [SerializeField] private GameObject adsHireButton;
        
        private UIFadeComponent fadeComponent;
        
        private string _offerId;
        private RecruitmentTavernSceneAdapter _adapter;
        private UIRootView _uiRoot;
        
        public void Bind(
            FacilityDTO facilityDto,
            TavernOfferDTO offer,
            RecruitmentTavernSceneAdapter adapter,
            UIRootView uiRoot,
            UIStyleResolver styleResolver,
            CharacterPortraitCache portraitCache)
        {
            _adapter = adapter;
            _uiRoot = uiRoot;
            _offerId = offer.Id;

            // эффект появления карточки
            fadeComponent = GetComponent<UIFadeComponent>();
            fadeComponent.Setup();
            fadeComponent.SetInstant(0f); // карточка изначально скрыта
            gameObject.GetChild(0).SetActive(false);
            
            // === Candidate
            nameText.text = offer.Name;
            levelText.text = $"Lvl. {offer.Level+1}";
            
            modelTexture.texture = portraitCache.GetFullBody(offer.ArchetypeId);
            

            // === Weapon
            BindEquipmentSlot(
                styleResolver,
                weaponSlot,
                offer.Weapon);

            // === Gear
            GearSlotDTO gear;
            TavernOfferEquipmentSlotView slot;
            var l = gearSlots.childCount;
            for (int i = 0; i < l; i++)
            {
                slot = gearSlots.GetChild(i).GetComponent<TavernOfferEquipmentSlotView>();
                if (i < offer.Gear.Count && !offer.Gear[i].Disable)
                {
                    gear = offer.Gear[i];

                    BindEquipmentSlot(
                        styleResolver,
                        slot,
                        gear);

                    slot.cRoot.SetActive(true);
                }
                else
                {
                    slot.cRoot.SetActive(false);
                }
            }

            // === Stats
            BindStats(offer);

            
            // === Hire button
            freeHireButton.SetActive(offer.PurchaseType == PurchaseType.Free);
            premiumHireButton.SetActive(offer.PurchaseType == PurchaseType.PremiumCurrency);
            premiumHireButton.GetChild(0).CMP_Text().text = offer.PremiumCost.ToString();
            adsHireButton.SetActive(offer.PurchaseType == PurchaseType.Ads);

            
            // === free
            freeHireButton.RegisterButtonClick(() =>
            {
                var response = _adapter.TryRecruit(_offerId, PurchaseType.Free);
                if (response.result.Success)
                    ServiceLocator.Current.Get<CoroutineController>()
                    .StartCoroutine(finishRecruit(response.finishAction, offer.Name, freeHireButton));
            });

            // === premium
            premiumHireButton.RegisterButtonClick(() =>
            {
                var data = new ConfirmPaidPopupData(
                    "Confirm Recruit",
                    "Are you sure you want to recruit this survivor?",
                    "Yes",
                    $"{offer.PremiumCost}",
                    onOk: () =>
                    {
                        var response = _adapter.TryRecruit(_offerId, PurchaseType.PremiumCurrency);
                        if (response.result.Success)
                            ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(finishRecruit(
                                    response.finishAction, offer.Name, premiumHireButton));
                    },
                    onClose: () => {}
                );

                ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPaidPopup, data);
            });

            // === ad
            adsHireButton.RegisterButtonClick(() =>
                {
                    if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService)
                    {
                        ServiceLocator.Current.Get<AdService>().OnGrantRewardEvent(p =>
                        {
                            var response = _adapter.TryRecruit(_offerId, PurchaseType.Ads);
                            if (response.result.Success)
                                ServiceLocator.Current.Get<CoroutineController>()
                                    .StartCoroutine(finishRecruit(response.finishAction, offer.Name, adsHireButton));
                        });
                    }
                },
                _adapter.NoFreeSlot
            );
            
            
            // добавляем появление
            fadeComponent.FadeIn(true, () => gameObject.GetChild(0).SetActive(true));
        }

        private void BindEquipmentSlot(
            UIStyleResolver styleResolver,
            TavernOfferEquipmentSlotView slot,
            GearSlotDTO dto)
        {
            slot.Bind(dto, styleResolver);
        }

        private void BindStats(TavernOfferDTO offer)
        {
            // var l = statRoot.childCount;
            // for (int i = 0; i < l; i++)
            // {
            //     statRoot.GetChild(i).gameObject.CMP_Text().text = 
            //         offer.Stats[i].Value.ToString();
            // }
        }
        

        IEnumerator finishRecruit(
            Action recruit, 
            string unitName, 
            GameObject button)
        {
            _uiRoot.EnableBlockScreen();
            button.SetActive(false);
            
            bool finished = false;
            fadeComponent.FadeOut(() => finished = true);

            yield return new WaitUntil(() => finished);
            

            if (recruit == null)
            {
                _uiRoot.DisableBlockScreen();
                yield break;
            }

            recruit();
            
            // todo
            // sound
            // animation
            ServiceLocator.Current.Get<INotificationService>().Push(
                new NotificationRequest(
                    "recruit",
                    $"{unitName} joined your camp!",
                    NotificationPriority.Normal,
                    NotificationChannel.Toast,
                    NotificationStyleCategory.Default,
                    new NotificationMessageConfig.NotificationStyle()));
            
            
            yield return new WaitForSeconds(.3f);
            _uiRoot.DisableBlockScreen();
        }
    }
}