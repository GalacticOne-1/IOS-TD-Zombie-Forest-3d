using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Items;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Production.DTO;
using Galactic1.Game.UI.Stats;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.Runtime.Production;
using Galactic1.UI.Core;
using UnityEngine;
using ValueType = Galactic1.Code.Items.ValueType;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Универсальный адаптер производственной станции.
    /// 
    /// Работает с любой реализацией IProductionStationRuntime:
    /// - Craft
    /// - Recycler
    /// - Smelter
    /// - etc.
    /// 
    /// Scene не знает конкретный тип станции.
    /// Runtime остаётся единственным источником истины.
    /// </summary>
    public sealed class UniversalProductionSceneAdapter : IFacilitySceneAdapter
    {
        private readonly IProductionStationRuntime _runtime;
        private readonly CraftingStationModule _stationConfig;
        private readonly IInventoryResourcesPort _inventory;
        private readonly IEconomyService _economy;
        private readonly ResourcesRequirementService _requirementService;
         
        public FacilityType Type => _runtime.Type;

        private RuntimeId _currentRecipe;
        public RuntimeId CurrentRecipe => _currentRecipe;

        
        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }

        public event Action OnCollectFailed;

        
        

        public void SetCurrentRecipe(RuntimeId id) => _currentRecipe = id;

        public UniversalProductionSceneAdapter(
            IProductionStationRuntime runtime,
            IInventoryResourcesPort inventory,
            IEconomyService economy)
        {
            _runtime = runtime;
            _inventory = inventory;
            _economy = economy;

            _stationConfig = _runtime.Config as CraftingStationModule;
            _requirementService = new ResourcesRequirementService(_inventory);
        }

        // =========================================================
        // RECIPE DETAILS
        // =========================================================

        public RecipeDetailsDto GetRecipeDetails(RuntimeId recipeId)
        {
            var recipeEntry = _stationConfig.AvailableRecipes.First(r => r.Id == recipeId);

            var recipe = recipeEntry.Recipes[0];

            
            // === мульти выход для recycler
            List<RecyclerOutputDTO> outputResources = new ();
            if (Type == FacilityType.Recycler)
            {
                
            }

            
            // === DESCRIPTER STATS
            List<StatDtoBase> descriptorDto = new();
            DescriptorStyleEntry descriptorEntry;
            var rawDescripter = recipe.OutputItem.GetDescriptors();
            foreach (var e in rawDescripter)
            {
                descriptorEntry = StatStyleResolver.Resolve(e.DescriptorId);

                if (descriptorEntry == null)
                {
                    Debug.LogError($"Not exist style for descriptor [{e.DescriptorId}]");
                    continue;
                }
                
                // * получаем layout если поле не статичное
                // var statEntry = descriptorEntry.layoutType == StatLayoutType.StaticLabel
                // ? null
                // : StatStyleResolver.Resolve(descriptorEntry.statId);
                var resolve = StatValueResolver.Resolve(e, out var statId);
                descriptorDto.Add(new DescriptorViewDto(
                    descriptorEntry,
                    resolve,
                    StatStyleResolver.Resolve(statId)
                ));
            }
            
            

            // === STATS GROUP
            List<StatGroupViewDto> statGroup = new();
            List<StatDtoBase> statDto = new();

            var rawStats = recipe.OutputItem.GetStats();
            foreach (var s in rawStats)
            {
                if (s.Value == 0)
                    continue;
                
                // * damage не показываем в поле, уже есть в статике
                if(s.StatId == StatId.Damage)
                    continue;
                
                
                var statEntry = StatStyleResolver.Resolve(s.StatId);
                if (statEntry != null)
                    statDto.Add(new StatViewDto(statEntry, s.Value));
                else
                    Debug.LogError($"Not exist style for stat [{s.StatId}]");
            }
            
            // === если есть список предметов 
            // например:
            //   - у оружия используемые боеприпасы
            //   - у патронов всё оружие которое юзает эти патроны
            if (recipe.OutputItem.TryGetLinkedItems(out var id, out var result))
                statDto.Add(new ItemListViewDto(StatStyleResolver.Resolve(id), result));

            
            
            // *** добавляем финальный список настроеных статов
            statGroup.Add(new StatGroupViewDto(
                "Main",
                "Stats",
                statDto));
            
            

            var requirements = recipe.Requirement
                .Select(r =>
                {
                    int owned = _inventory.GetTotalAmount(r.Item.Id);
                    bool enough = owned >= r.Amount;

                    return new RecipeRequirementDto(
                        r.Item.Id,
                        r.Item,
                        r.Item.Header.icon,
                        r.Amount,
                        owned,
                        enough);
                })
                .ToList();

            bool canAdd = _runtime.CanAddJob(recipeId) &&
                          _requirementService.HasResources(recipe.Requirement);

            bool requiresStationUpgrade = !recipe.CanCraftAtLevel(_runtime.Level);

            return new RecipeDetailsDto(
                recipeId,
                recipe.OutputItem.Header.titleLid,
                recipe.OutputItem.Header.icon,
                recipe.OutputItem.Classification.category,
                recipe.OutputItem.Classification.rarity,
                recipe.OutputCount,
                outputResources,
                (int)recipe.CraftTime,
                requirements,
                descriptorDto,
                statGroup,
                canAdd,
                !requiresStationUpgrade,
                new RecipeDetailsDto.StationUpgradeCtx
                {
                    requiresBlueprint = false,
                    requiresStationUpgrade = requiresStationUpgrade,
                    stationAlertMessage =
                        $"Upgrade {_stationConfig.Item.Header.titleLid} Lvl. {(byte)recipe.RequiredTier}"
                });
        }
        
        // =========================================================
        // COMMANDS
        // =========================================================

        public bool TryAddOrder(RuntimeId recipeId)
        {
            if (recipeId== null)
                return false;

            if (!_runtime.CanAddJob(recipeId))
                return false;

            var recipe = _stationConfig.AvailableRecipes.First(r => r.Id == recipeId).Recipes[0];

            if (!_requirementService.HasResources(recipe.Requirement))
                return false;

            // списание ресурсов (после проверки)
            foreach (var r in recipe.Requirement)
                _inventory.TrySpend(r.Item.Id, r.Amount);

            return _runtime.TryAddJob(
                recipeId,
                (int)recipe.CraftTime,
                1,
                recipe.StackOrderLimit,
            recipe.OutputCount);
        }

        public bool CancelOrder(string jobId)
        {
            var job = _runtime.GetQueueDTO().FirstOrDefault(j => j.JobId == jobId);

            if (job == null)
                return false;

            var recipeEntry = _stationConfig.AvailableRecipes
                .FirstOrDefault(r => r.Id == job.RecipeId);

            if (recipeEntry == null)
                return false;

            var recipe = recipeEntry.Recipes[0];

            // === 1. Проверка вместимости
            var refundSlots = recipe.Requirement
                .Select(r => new InventorySlotRuntime(
                    GameContent.Items.Get(r.Item.Id),
                    r.Amount,
                    0,
                    0))
                .ToList();

            if (!_inventory.CanAddMultiple(refundSlots))
            {
                OnCollectFailed?.Invoke();
                return false;
            }

            // === 2. Реальное добавление
            foreach (var slot in refundSlots)
                _inventory.TryAdd(slot);

            // === 3. Удаление заказа
            return _runtime.CancelJob(jobId);
        }

        /// <summary>
        /// Забирает предметы со всех готовых слотов
        /// </summary>
        public void CollectCompleted()
        {
            var jobs = _runtime.GetQueueDTO()
                .Where(j => j.CompletedStack > 0)
                .ToList();

            if (jobs.Count == 0)
                return;

            bool failed = false;

            foreach (var job in jobs)
            {
                int totalItems = job.CompletedStack * job.Amount;

                var item = GameContent.Items.Get(job.RecipeId);
                var slot = new InventorySlotRuntime(item, totalItems, item.Physical.maxDurability, 0);
                
                if (!_inventory.CanAdd(slot))
                {
                    OnCollectFailed?.Invoke();
                    return;
                }

                var result = _inventory.TryAdd(slot);

                int added = totalItems - result.Remaining;

                if (added > 0)
                {
                    int orders = added / job.Amount;
                    _runtime.CollectCompletedOrders(job.JobId, orders);
                }

                if (result.Remaining > 0)
                    failed = true;
            }

            if (failed)
                OnCollectFailed?.Invoke();
        }
        
        public void CollectSingle(string jobId)
        {
            var job = _runtime.GetQueueDTO().FirstOrDefault(j => j.JobId == jobId);
            if (job == null)
                return;

            if (job.CompletedStack <= 0)
                return;

            var item = GameContent.Items.Get(job.RecipeId);

            var slot = new InventorySlotRuntime(item, job.Amount, item.Physical.maxDurability, 0);

            if (!_inventory.CanAdd(slot))
            {
                OnCollectFailed?.Invoke();
                return;
            }

            var result = _inventory.TryAdd(slot);

            if (result.Remaining == 0)
            {
                _runtime.CollectCompletedOrders(jobId, 1);
            }
            else
            {
                OnCollectFailed?.Invoke();
            }
        }

        public bool TryPaidSkip()
        {
            int remaining = CalculateRemaining();
            int cost = _economy.CalculateProductionSkipCost(
                remaining,
                _runtime.Level);

            if (!_economy.TrySpend(EBankResourceType.CurrencyPremium, cost))
                return false;

            _runtime.SkipActive();
            return true;
        }

        // =========================================================
        // INTERNAL
        // =========================================================
        
        public IReadOnlyList<ProductionJobRuntimeDTO> GetQueue()
        {
            return _runtime.GetQueueDTO();
        }

        private int CalculateRemaining()
        {
            var queue = _runtime.GetQueueDTO();

            if (queue.Count == 0)
                return 0;

            var active = queue[0];

            if (active.State != ProductionJobState.InProgress)
                return 0;

            int remaining =
                active.StartWorldHour + active.TotalDurationHours
                - _runtime.TotalWorldHour;

            return Math.Max(0, remaining);
        }

        private int CalculateTotalRemaining(int activeRemaining)
        {
            int total = 0;

            foreach (var job in _runtime.GetQueueDTO())
            {
                if (job.State == ProductionJobState.InProgress)
                    total += activeRemaining;
                else if (job.State == ProductionJobState.Queued)
                    total += job.TotalDurationHours;
            }

            return total;
        }
    }
}