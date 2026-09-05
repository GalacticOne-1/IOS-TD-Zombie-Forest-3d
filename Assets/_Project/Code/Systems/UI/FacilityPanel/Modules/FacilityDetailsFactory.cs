
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.Game.UI.Production;
using Galactic1.Game.UI.Production.DTO;
using Galactic1.Items;
using Galactic1.Runtime.Production;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.Game.UI.Buildings
{
    /// <summary>
    /// Создаёт специализированные BuildingDetailsDTO.
    /// </summary>
    public sealed class FacilityDetailsFactory
    {
        private readonly ICampCapacityService _capacityService;
        private readonly IEconomyService _economyService;


        public FacilityDetailsFactory(
            ICampCapacityService capacityService,
            IEconomyService economyService
        )
        {
            _capacityService = capacityService;
            _economyService = economyService;
        }

        public IFacilityDetailsDTO Create(BaseCampFacilityRuntime runtime)
        {
            switch (runtime)
            {
                case IInboxFacilityRuntime inbox:
                    return BuildMainContainer(inbox);
                
                case IRecruitmentTavernRuntime tavern:
                    return BuildTavern(tavern);
                
                case ILivingModuleFacilityRuntime livingModule:
                    return BuildLivingModule(livingModule);
                
                case IGarageFacilityRuntime garage:
                    return BuildGarage(garage);
                
                
                
                case IProductionStationRuntime production:
                    return BuildProduction(production);
                
                case IStorageFacilityRuntime storage:
                    return BuildStorage(storage);
                
                
                case ICombatFacilityRuntime combat:
                    return BuildCombat(combat);

                default:
                    return null;
            }
        }

        #region Production

        public ProductionFacilityDetailsDTO BuildProduction(IProductionStationRuntime runtime)
        {
            var config = runtime.Config;
            int currentHour = runtime.TotalWorldHour;

            // === Recipes
            var recipes = config.Item.CraftStation
                .AvailableRecipes
                .Select(recipe => new RecipeCardData(
                    recipe.Id,
                    recipe.Header.icon,
                    recipe.Classification.rarity,
                    recipe.Header.titleLid,
                    recipe.Recipes[0].CanCraftAtLevel(runtime.Level)))
                .ToList();

            // === Queue
            var queue = runtime.GetQueueDTO()
                .Select(job =>
                {
                    var item = GameContent.Items.Get(job.RecipeId);

                    int remaining = 0;

                    if (job.State == ProductionJobState.InProgress)
                    {
                        remaining = Math.Max(0, job.TotalDurationHours - (currentHour - job.StartWorldHour));
                    }

                    return new ProductionJobDTO(
                        job.JobId,
                        item.Header.icon,
                        item.Classification.rarity,
                        job.TotalDurationHours,
                        remaining,
                        job.Amount,
                        job.CurrentStack,
                        job.CompletedStack,
                        job.State
                    );
                })
                .ToList();

            int remainingActive = ProductionTimeCalculator.CalculateRemaining(runtime);
            int totalRemaining = ProductionTimeCalculator.CalculateTotalRemaining(runtime, remainingActive);
            
            
            bool hasActiveProduction = runtime.GetQueueDTO()
                .FirstOrDefault()?.State == ProductionJobState.InProgress;

            var skipCost = _economyService.CalculateProductionSkipCost(
                remainingActive,
                runtime.Level);

            var canSkip = _economyService.HasEnough(
                EBankResourceType.CurrencyPremium,
                skipCost);

            bool hasCompleted = runtime.GetQueueDTO()
                .Any(j => j.CompletedStack > 0);

            return new ProductionFacilityDetailsDTO(
                config.FacilityType,
                queue,
                recipes,
                new RecipeDetailsDto(),
                hasCompleted,
                totalRemaining,
                hasActiveProduction,
                skipCost,
                canSkip
            );
        }


        #endregion

        StorageFacilityDetailsDTO BuildStorage(IStorageFacilityRuntime runtime)
        {
            var module = runtime.Module;
            return new StorageFacilityDetailsDTO(
                module.FacilityType,
                module.StorageType,
                module.Capacity,
                module.SpecialDescription);
        }
        

        #region Tavern

        TavernDetailsDTO BuildTavern(IRecruitmentTavernRuntime runtime)
        {
            var offersList = new List<TavernOfferDTO>();

            foreach (var offer in runtime.Offers)
            {
                if (offer == null || 
                    !GameContent.ResolveItem(offer.Equipment.WeaponItem.Id, out var gearItem))
                    continue;


                // === Собираем Weapon
                var maxDurability = gearItem.Physical.maxDurability;
                var weaponDTO = new GearSlotDTO()
                {
                    Icon = gearItem.Header.icon,
                    Durability = offer.Equipment.WeaponItem.Durability,
                    DurabilityPrcnt = Mathf.CeilToInt(((float)offer.Equipment.WeaponItem.Durability / maxDurability) * 100),
                    Durability01 = (float)offer.Equipment.WeaponItem.Durability / maxDurability,
                    Rarity = gearItem.Classification.rarity,
                    Item = gearItem
                };

                // === Собираем Gear
                var gearDtoList = new List<GearSlotDTO>();
                var l = offer.Equipment.ArmorItem.Count;
                for (int i = 0; i < l; i++)
                {
                    // что бы пустые части не ломали порядок
                    if (!GameContent.ResolveItem(offer.Equipment.ArmorItem[i].Id, out gearItem))
                    {
                        gearDtoList.Add(new() { Disable = true });
                        continue;
                    }

                    maxDurability = gearItem.Physical.maxDurability;
                    gearDtoList.Add(new()
                    {
                        Icon = gearItem.Header.icon,
                        Durability = offer.Equipment.ArmorItem[i].Durability,
                        DurabilityPrcnt = Mathf.CeilToInt(((float)offer.Equipment.ArmorItem[i].Durability / maxDurability) * 100),
                        Durability01 = (float)offer.Equipment.ArmorItem[i].Durability / maxDurability,
                        Rarity = gearItem.Classification.rarity,
                        Item = gearItem
                    });
                }


                // Собираем Stats
                var statsDtoList = new List<StatDTO>();
                // if (offer.Archetype?.BaseStats != null)
                // {
                //     foreach (var stat in offer.Archetype.BaseStats)
                //     {
                //         statsDtoList.Add(new StatDTO
                //         {
                //             //StatName = kv.Key,
                //             Value = stat
                //         });
                //     }
                // }

                var offerDto = new TavernOfferDTO
                {
                    Id = offer.Id,
                    Name = offer.Identity.DisplayName,
                    ArchetypeId = offer.Identity.ArchetypeId,
                    Level = offer.Level,

                    Weapon = weaponDTO,
                    Gear = gearDtoList,
                    Stats = statsDtoList,
                    
                    CanHire = runtime.CanRecruit(offer.Id),
                    PurchaseType = offer.PurchaseType,
                    PremiumCost = offer.PremiumCost
                };

                offersList.Add(offerDto);
            }

            return new TavernDetailsDTO(
                _capacityService.GetMaxCapacity(),
                _capacityService.GetCurrentUnits(),
                runtime.DaysUntilRefresh,
                offersList
            );
        }


        #endregion


        InboxModuleDetailsDTO BuildMainContainer(IInboxFacilityRuntime runtime)
        {
            var slots = runtime.Inbox.Slots;

            var list = new List<InboxItemDTO>(slots.Count);

            for (int i = slots.Count - 1; i >= 0; i--)
            {
                var slot = slots[i];
                var item = slot.Item.Value;

                if (item == null)
                    continue;

               
                int maxDurability = item.Physical.maxDurability;
                int durability = (int)(slot.Durability.Value.PercentFrom(maxDurability) * 100);

                float durability01 = maxDurability > 0
                    ? slot.Durability.Value / (float)maxDurability
                    : 1f;
                
                int remainingHours = Math.Max(
                    0,
                    slot.ExpireWorldHour - runtime.TotalWorldHour);

                list.Add(new InboxItemDTO
                {
                    SlotId = slot.SlotId,
                    Item = slot.Item.Value,
                    Count = slot.Amount.Value,

                    DurabilityCurrent = durability,
                    Durability01 = durability01,

                    RemainingHours = remainingHours
                });
            }

            return new InboxModuleDetailsDTO(list);
        }
        
        GarageDetailsDTO BuildGarage(IGarageFacilityRuntime runtime)
        {
            var vehicles = GameContent.Items.GetAllTransport().ToList();
            var equippedId = runtime.GetCurrentVehicleId();

            var config = GameContent.Items.Get(runtime.GetCurrentVehicleId());

            return new GarageDetailsDTO(vehicles, equippedId, config.PreviewConfig, config.PrefabPath);
        }
            


        LivingModuleDetailsDTO BuildLivingModule(ILivingModuleFacilityRuntime runtime)
        {
            return new(); // не заполняется из кода, зашито в самой панели
        }
        
        
        CombatDetailsDTO BuildCombat(ICombatFacilityRuntime runtime)
        {
            var config = runtime.Config.Item;

            var hp = 0;
            var damage = 0f;

            if (config.HasModule<BuildingHealthModule>())
                hp = config.BuildingHealth.Settings.maxHealth;

            if (config.HasModule<BuildingAttackModule>())
                damage = config.BuildingAttack.WeaponDefinition.damage;
            
            

            return new(hp, damage);
        }
    }
}