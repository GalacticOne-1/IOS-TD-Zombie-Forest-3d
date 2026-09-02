using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Game.Rewards;
using Galactic1.Code.Systems.Ads;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.UI.Core;
using Galactic1.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Контроллер панели отчёта после рейда.
    /// Отвечает только за отображение данных:
    /// - название локации
    /// - список бойцов
    /// - список полученного лута
    /// </summary>
    public class RaidReportController : UIScreenPanel
    {
        [Header("Header")] 
        [SerializeField] private TMP_Text locationTitleText;

        [Header("Lists")] 
        [SerializeField] private ScrollRect scrollSurvivor;
        [SerializeField] private Transform survivorRoot;
        [SerializeField] private GameObject survivorPrefab;
        [SerializeField] private ScrollRect scrollLoot;
        [SerializeField] private Transform lootRoot;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private GameObject resourceDividePrefab;
        [SerializeField] private GameObject lootDividePrefab;

        [Header("Ad Offer")] 
        [SerializeField] private GameObject adOfferBlock;
        [SerializeField] private TMP_Text adBonusDescriptionText;
        [SerializeField] private TMP_Text bonusItemsText;
        [SerializeField] private GameObject adDeal, adAlert;
        [SerializeField] private GameObject adButton;
        [SerializeField] private GameObject continueButton;

        private Action<bool> _onNext; // true = реклама просмотрена
        private bool _adWatched;
        private UIStyleResolver _style;

        private readonly List<RaidLootItemView> _lootViews = new();
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            _style = ServiceLocator.Current.Get<UIStyleResolver>();

            adButton.RegisterButtonClick(OnAdButtonClick);
            continueButton.RegisterButtonClick(OnContinueClick);
        }

        public void Show(RaidReportData data, Action<bool> onNext)
        {
            gameObject.SetActive(true);
            _onNext = onNext;
            _adWatched = false;

            locationTitleText.text = $"Raid report: {data.LocationTitle}";

            UpdateSurvivors(data.Survivors);
            UpdateLoot(data.Loot, data.AdBonusAvail);

            AdBox(data);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            survivorRoot.MakeEmpty();
            lootRoot.MakeEmpty();
        }

        private void OnAdButtonClick()
        {
            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService)
            {
                ServiceLocator.Current.Get<AdService>().OnGrantRewardEvent((placement) =>
                {
                    _adWatched = true;
                    adButton.SetActive(false);
                    
                    foreach (var view in _lootViews)
                        view.ApplyAdBonus();
                });
            }
        }

        private void OnContinueClick() => _onNext?.Invoke(_adWatched);

        // список юнитов
        private void UpdateSurvivors(IReadOnlyList<RaidSurvivorResult> survivors)
        {
            if (survivors == null || survivors.Count == 0) return;
            foreach (var s in survivors)
            {
                var view = survivorPrefab.CreateGO(survivorRoot).GetComponent<RaidSurvivorItemView>();
                view.Bind(s);
            }

            scrollSurvivor.SetSizeContentLayoutGroup(true, survivorRoot, true);
        }

        
        // список с лутом
        private void UpdateLoot(List<RaidLootResult> loot, bool adBonusAvail)
        {
            if (loot == null || loot.Count == 0) return;

            _lootViews.Clear();
            var resources = loot.Where(l =>
                l.Item.Classification.itemLabel == ItemLabel.Resource).ToList();
            var items = loot.Where(l =>
                l.Item.Classification.itemLabel == ItemLabel.Loot).ToList();

            if (resources.Count > 0)
            {
                resourceDividePrefab.CreateGO(lootRoot);
                foreach (var r in resources)
                {
                    var view = lootPrefab.CreateGO(lootRoot).GetComponent<RaidLootItemView>();

                    view.Bind(r, _style, adBonusAvail);

                    _lootViews.Add(view);
                }
            }

            if (items.Count > 0)
            {
                lootDividePrefab.CreateGO(lootRoot);
                foreach (var i in items)
                    lootPrefab.CreateGO(lootRoot)
                        .GetComponent<RaidLootItemView>()
                        .Bind(i, _style, adBonusAvail);
            }

            scrollLoot.SetSizeContentLayoutGroup(true, lootRoot, true);
            scrollLoot.ScrollRectResetV();
            
            //DebugAdBonus();
        }


        void AdBox(RaidReportData data)
        {
            // Показываем блок рекламы только если eligible
            adDeal.SetActive(data.AdBonusAvail);
            adAlert.SetActive(!data.AdBonusAvail);
            if (data.AdBonusAvail)
            {
                var mult = ServiceLocator.Current.Get<IAdRewardProvider>().GetAdMultiplier(AdPlacement.PostRaid);
                adBonusDescriptionText.text = $"Loot +{(mult - 1) * 100:0}%";
            }

            bonusItemsText.text = !data.AdBonusAvail
                ? ""
                : TextBuilder.Start()
                    .Size(130)
                    .Text("[")
                    .End()
                    .Size(80)
                    .Text($"AD Bonus ")
                    .End()
                    .Size(130)
                    .Text($"+{data.BonusLootCount}]")
                    .End()
                    .ToString();
            
            adAlert.CMP_Text().text = data.LootEmpty
                ? "No loot, bonus unavailable."
                : "Transport is full.\nThe bonus cannot be applied!";
        }

        void DebugAdBonus()
        {
            DebugInputService.I.On(KeyCode.A, () =>
            {
                foreach (var view in _lootViews)
                    view.ApplyAdBonus();
            });
        }
    }
}
