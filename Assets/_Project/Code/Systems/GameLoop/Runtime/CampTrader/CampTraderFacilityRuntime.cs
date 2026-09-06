using System;
using System.Linq;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Meta.Configs.Trader;
using Galactic1.UI.Audio;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime здания трейдера.
    /// В отличие от таверны — офферы статичны (берутся напрямую из CampTraderConfig),
    /// генерации / refresh-цикла нет.
    /// </summary>
    public sealed class CampTraderFacilityRuntime :
        BaseCampFacilityRuntime,
        ICampTraderRuntime
    {
        private readonly CampTraderConfig _traderConfig;
        private readonly IEconomyService _economy;

        public override FacilityType Type => FacilityType.CampTrader;
        public override bool CanUpgrade => false;

        public System.Collections.Generic.IReadOnlyList<TraderOfferConfig> Offers => _traderConfig.Offers;

        private CampTraderPanelAudioConfig audioConfig;


        public CampTraderFacilityRuntime(
            FacilityProxy proxy,
            FacilityModule config,
            GameTimeService timeService,
            CampTraderConfig traderConfig,
            IEconomyService economy)
            : base(proxy, config, timeService)
        {
            _traderConfig = traderConfig;
            _economy = economy;

            audioConfig = ServiceLocator.Current.Get<ConfigProvider>()
                .Get<UIAudioDatabase>()
                .Get<CampTraderPanelAudioConfig>("camp_trader_panel_audio");
        }

        public override void Dispose() {}


        public bool CanBuy(string offerId, PurchaseType type)
        {
            var offer = FindOffer(offerId);
            if (offer == null)
                return false;

            return CanPay(offer, type);
        }

        public (NotificationResult, Action) TryBuy(string offerId, PurchaseType type)
        {
            var offer = FindOffer(offerId);

            if (offer == null)
                return (NotificationResult.Fail(NotificationFailReason.None), null);

            if (!CanPay(offer, type))
                return (NotificationResult.Fail(NotificationFailReason.NotEnoughPremiumCurrency), null);

            if (!Spend(offer, type))
                return (NotificationResult.Fail(NotificationFailReason.NotEnoughPremiumCurrency), null);

            // === выдача предмета вынесена в делегат, что бы UI сам завершил
            // (тот же паттерн, что и finishRecruit в таверне)
            Action finishBuy = () =>
            {
                GrantItemStub(offer);
                MarkStateChanged();
            };

            return (NotificationResult.Ok(), finishBuy);
        }


        private TraderOfferConfig FindOffer(string offerId)
            => _traderConfig.Offers.FirstOrDefault(
                o => o.Item != null && o.Item.Id.Guid == offerId);


        private bool CanPay(TraderOfferConfig offer, PurchaseType type)
        {
            return type switch
            {
                PurchaseType.PremiumCurrency =>
                    _economy.HasEnough(EBankResourceType.CurrencyPremium, offer.Cost),

                PurchaseType.Ads => true, // проверка будет через AdsService

                _ => false
            };
        }

        private bool Spend(TraderOfferConfig offer, PurchaseType type)
        {
            return type switch
            {
                PurchaseType.PremiumCurrency =>
                    _economy.TrySpend(EBankResourceType.CurrencyPremium, offer.Cost),

                PurchaseType.Ads => true, // списание не требуется, выдача после просмотра рекламы

                _ => false
            };
        }

        /// <summary>
        /// Выдача предмета
        /// </summary>
        /// <param name="offer"></param>
        private void GrantItemStub(TraderOfferConfig offer)
        {
            EventBus<AudioUIEvent>.Raise(new AudioUIEvent(audioConfig.buy.ToData()));
            
            ServiceLocator.Current.Get<InboxService>().AddReward(
                new InventorySlotRuntime(
                    offer.Item,
                    offer.Amount,
                    offer.Durability,
                    0));
        }
    }
}
