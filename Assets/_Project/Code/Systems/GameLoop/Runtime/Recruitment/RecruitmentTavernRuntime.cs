using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Economy.Configs;
using Galactic1.Code.Systems.Runtime.Recruitment;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.Structs;
using Galactic1.UI.CharacterPreview;
using Galactic1.Utility;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime здания найма (таверна).
    /// Полностью автономная логика генерации и найма.
    /// </summary>
    public sealed class RecruitmentTavernRuntime : 
        BaseCampFacilityRuntime,
        IRecruitmentTavernRuntime
    {
        private readonly RecruitmentDatabase _database;
        private readonly RecruitmentSettingsConfig _settings;
        private readonly ICampCapacityService _capacity;
        private readonly IEconomyService _economy;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IWeightedRandomService _rng;
        private readonly IRecruitEquipmentGenerator _equipmentGenerator;
        private readonly RecruitOfferFactory _recruitOfferFactory;

        public override FacilityType Type => FacilityType.Tavern;
        public override bool CanUpgrade => false;
        
        
        

        private int _nextRefreshDay => Proxy.NextRefreshDay.Value;

        public IReadOnlyList<RecruitOfferProxy> Offers => Proxy.TavernOffers;


        private readonly int recruitmentPremiumCost;
        private readonly int refreshPremiumCost;
        public int RefreshPremiumCost => refreshPremiumCost;
        
        public int DaysUntilRefresh
        {
            get
            {
                int daysLeft = _nextRefreshDay - TimeService.CurrentDay;
                return daysLeft < 0 ? 0 : daysLeft;
            }
        }
        
        
        
        
        
        
        
        public RecruitmentTavernRuntime(
            FacilityProxy proxy,
            TavernModule config,
            GameTimeService timeService,
            RecruitmentDatabase database,
            RecruitmentSettingsConfig settings,
            ICampCapacityService capacity,
            IEconomyService economy,
            EconomyConfig economyConfig,
            IIdentityGenerator identityGenerator,
            IWeightedRandomService rng, 
            IRecruitEquipmentGenerator equipmentGenerator)
            : base(proxy, config, timeService)
        {
            _database = database;
            _settings = settings;
            _capacity = capacity;
            _economy = economy;
            _identityGenerator = identityGenerator;
            _rng = rng;
            _equipmentGenerator = equipmentGenerator;
            _recruitOfferFactory = new RecruitOfferFactory();

            recruitmentPremiumCost = economyConfig.RecruitPremium;
            refreshPremiumCost = economyConfig.RefreshPremium;

            TimeService.DayPassed += OnDayPassed;

            // === добавляем первые оферы, потом обновление будет каждые 3 дня
            if (!Proxy.IsWorking.Value)
            {
                Proxy.IsWorking.Value = true;
                GenerateOffers(false);
            }
        }

        public override void Dispose(){}
        
        
        
        private void OnDayPassed(DayPassedEvent evt)
        {
            if (evt.Day >= _nextRefreshDay)
                GenerateOffers();
        }

        private void GenerateOffers(bool createTexture = true)
        {
            var list = new List<RecruitOfferData>();
            
            // --- Common
            for (int i = 0; i < _settings.CommonOffersCount; i++)
                list.Add(GenerateCommon(list));

            // --- Experienced
            for (int i = 0; i < _settings.ExperiencedOffersCount; i++)
                list.Add(GenerateExperienced(list, i == 0));

            // --- Specialist
            for (int i = 0; i < _settings.SpecialistOffersCount; i++)
                list.Add(GenerateSpecialist(list));

            // === create texture
            if(createTexture) // при первой загрузке игры таверна текстуру не делает, иначе баги
            {
                ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                    ServiceLocator.Current.Get<ConfigProvider>().Get<UnitIdentityPoolConfig>(),
                    ServiceLocator.Current.Get<CharacterPreviewService>(),
                    list.Select(_ => _.Identity.ArchetypeId).ToList(),
                    () =>
                    {
                        Proxy.SetTavernOffers(list);
                        Proxy.NextRefreshDay.Value = TimeService.CurrentDay + _settings.RefreshIntervalDays;
                        MarkStateChanged();
                    }
                );
            }
            else
            {
                Proxy.SetTavernOffers(list);
                Proxy.NextRefreshDay.Value = TimeService.CurrentDay + _settings.RefreshIntervalDays;
                MarkStateChanged();
            }
        }
        

        private RecruitOfferData GenerateCommon(List<RecruitOfferData> alreadyGenerated)
        {
            var archetype = _rng.PickWeighted(_database.CommonArchetypes, _ => 1);
            var equipment = _equipmentGenerator.Generate(RecruitCategory.Common, archetype, 0);
            
            // Передаём занятые архетипы в генератор
            var identity = _identityGenerator.Generate(GetUsedArchetypeIds(alreadyGenerated));

            return _recruitOfferFactory.CreateCommon(archetype, identity, equipment);
        }

        private RecruitOfferData GenerateExperienced(List<RecruitOfferData> alreadyGenerated, bool forAds)
        {
            var archetype = _rng.PickWeighted(_database.ExperiencedArchetypes, _ => 1);
            
            int level = _rng.Range(
                _settings.ExperiencedMinLevel,
                _settings.ExperiencedMaxLevel + 1);

            var equipment = _equipmentGenerator.Generate(RecruitCategory.Experienced, archetype, level);
            
            // Передаём занятые архетипы в генератор
            var identity = _identityGenerator.Generate(GetUsedArchetypeIds(alreadyGenerated));

            return _recruitOfferFactory.CreateExperienced(
                archetype,
                identity,
                level,
                forAds ? PurchaseType.Ads : PurchaseType.PremiumCurrency,
                forAds ? 0 : recruitmentPremiumCost,
                equipment);
        }

        private RecruitOfferData GenerateSpecialist(List<RecruitOfferData> alreadyGenerated)
        {
            var pipeline = new SpecialistGenerationPipeline(
                _database.SpecialistArchetypes,
                _identityGenerator,
                _rng,
                _equipmentGenerator,
                _settings);

            return pipeline.Generate();
        }
        
        
        /// <summary>
        /// Собирает занятые архетипы из:
        /// 1. уже сгенерированных офферов в текущей сессии
        /// 2. существующих юнитов игрока
        /// </summary>
        private IReadOnlyCollection<string> GetUsedArchetypeIds(
            List<RecruitOfferData> alreadyGenerated = null)
        {
            var used = new HashSet<string>();

            // Уже сгенерированные в этой сессии
            if (alreadyGenerated != null)
                foreach (var offer in alreadyGenerated)
                    if (!string.IsNullOrEmpty(offer.Identity.ArchetypeId))
                        used.Add(offer.Identity.ArchetypeId);
            
            // Текущие из прокси
            foreach (var offer in Proxy.TavernOffers)
                if (!string.IsNullOrEmpty(offer.Identity.ArchetypeId))
                    used.Add(offer.Identity.ArchetypeId);

            // Архетипы существующих юнитов
            var gameLoop = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            foreach (var unit in gameLoop.PlayerUnits)
                if (!string.IsNullOrEmpty(unit.ArchetypeId))
                    used.Add(unit.ArchetypeId);

            return used;
        }
        
        
        
        
        
        public bool CanRefreshPremium()
            => _economy.HasEnough(EBankResourceType.CurrencyPremium, refreshPremiumCost);

        
        public bool TryPremiumRefresh()
        {
            if (!CanRefreshPremium())
                return false;

            if (!_economy.TrySpend(EBankResourceType.CurrencyPremium, refreshPremiumCost))
                return false;

            GenerateOffers();
            return true;
        }



        public bool HasFreeSlot() => _capacity.HasFreeSlot();

        public bool CanRecruit(string offerId)
        {
            return true;
        }


        public (NotificationResult, Action ) TryRecruit(string offerId, PurchaseType hireType)
        {
            var offer = Proxy.TavernOffers.FirstOrDefault(o => o.Id == offerId);
            
            if (offer == null)
                return (NotificationResult.Fail(NotificationFailReason.None), null);

            if (!_capacity.HasFreeSlot())
                return (NotificationResult.Fail(NotificationFailReason.NoFreeCampSlots), null);

            if (!CanPay(offer, hireType))
                return (NotificationResult.Fail(NotificationFailReason.NotEnoughPremiumCurrency), null);

            if (!Spend(offer, hireType))
                return (NotificationResult.Fail(NotificationFailReason.NotEnoughPremiumCurrency), null);

            // === создание нового юнита, вынес в делегат что бы view сам закончил
            Action finishRecruit = () =>
            {
                var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
                var statsDefault = configProvider.Get<PlayerStatsBase>();
                ItemConfig item;
                
                // === маппинг экипировки в слоты
                var equipment = new Dictionary<EquipSlotType, InventorySlotData>();
                var loadout = offer.Equipment;

                // слот 0 = WeaponMain
                if (GameContent.ResolveItem(loadout.WeaponItem.Id, out item))
                {
                    if (item.Weapon != null)
                        equipment.Add(
                            item.Weapon.Settings.slotType, 
                            new InventorySlotData(
                                item, 
                                loadout.WeaponItem.Id,
                                1, 
                                loadout.WeaponItem.Durability,
                                loadout.WeaponItem.AmmoInMagazine));
                }

                // слоты 6-9 = Head, Body, Pants, Legs
                foreach (var armor in loadout.ArmorItem)
                {
                    if (GameContent.ResolveItem(armor.Id, out item))
                    {
                        if (item.Equipment != null)
                            equipment.Add(
                                item.Equipment.Settings.slotType,
                                new InventorySlotData(item, armor.Id, 1, armor.Durability, 0));
                    }
                }

                var playerProxy = new PlayerProxy(new PlayerData()
                {
                    Id = offer.Id,
                    Name = offer.Identity.DisplayName,
                    ArchetypeId = offer.Identity.ArchetypeId,
                    Level = offer.Level,

                    Stats = DictionaryUtility.ToList(statsDefault.GetBaseStats()),
                    Inventory = new List<InventorySlotData>(),
                    Equipment = new List<InventorySlotData>()

                });
                
                ServiceLocator.Current.Get<GameSession>().GameLoopContext.CreateUnitCompletely(playerProxy, equipment);

                // === добавляем портрет
                ServiceLocator.Current.Get<CharacterPortraitCache>().Warmup(
                    configProvider.Get<UnitIdentityPoolConfig>(),
                    ServiceLocator.Current.Get<CharacterPreviewService>(),
                    offer.Identity.ArchetypeId
                );
                
                // ********************
                Proxy.RemoveOffer(offer);
                MarkStateChanged();
            };
           
            return (NotificationResult.Ok(), finishRecruit);
        }







        private bool CanPay(RecruitOfferProxy offer, PurchaseType type)
        {
            return type switch
            {
                PurchaseType.Free => offer.Category == RecruitCategory.Common,

                PurchaseType.PremiumCurrency =>
                    _economy.HasEnough(EBankResourceType.CurrencyPremium, offer.PremiumCost),

                PurchaseType.Ads => true, // проверка будет через AdsService

                _ => false
            };
        }

        private bool Spend(RecruitOfferProxy offer, PurchaseType type)
        {
            return type switch
            {
                PurchaseType.Free => true,

                PurchaseType.PremiumCurrency =>
                    _economy.TrySpend(EBankResourceType.CurrencyPremium, offer.PremiumCost),

                PurchaseType.Ads => true, // списание произойдёт после рекламы

                _ => false
            };
        }

    }
}