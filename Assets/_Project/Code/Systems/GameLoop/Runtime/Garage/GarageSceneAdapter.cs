using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Garage;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Garage.DTO;
using Galactic1.Game.UI.Stats;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.Items;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime
{
    public class GarageSceneAdapter : IFacilitySceneAdapter
    {
        private readonly GameLoopContext _gameLoopContext;
        private readonly IGarageFacilityRuntime _runtime;
        private readonly IEconomyService _economy;
        private readonly IInventoryResourcesPort _inventory;
        private readonly ItemDatabase _itemDatabase;
        private readonly ResourcesRequirementService _requirementService;

        public FacilityType Type => _runtime.Type;
        
        private RuntimeId _currentModule;
        public RuntimeId CurrentModule => _currentModule;


        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }

        public event Action OnApplyFailed;
        
        
        public void SetCurrentModule(RuntimeId id) => _currentModule = id;


        public GarageSceneAdapter(
            GameLoopContext gameLoopContext,
            IGarageFacilityRuntime runtime,
            IInventoryResourcesPort inventory,
            IEconomyService economy,
            ItemDatabase itemDatabase)
        {
            _gameLoopContext = gameLoopContext;
            _runtime = runtime;
            _economy = economy;
            _inventory = inventory;
            _itemDatabase = itemDatabase;
            _requirementService = new ResourcesRequirementService(_inventory);
        }
        
        public GarageModuleDetailsDTO GetModuleDetails(RuntimeId moduleId)
        {
            if (!GameContent.Items.TryGet(moduleId, out var item))
                return default;
            
            var recipe = item.Recipes.FirstOrDefault();
            List<ModuleRequirementDto> requirements = new();

            // Получаем требования из ItemConfig / CraftModule
            if (recipe != null)
            {
                requirements = recipe.Requirement
                    .Select(r =>
                    {
                        int owned = _inventory.GetTotalAmount(r.Item.Id);

                        return new ModuleRequirementDto(
                            r.Item.Id,
                            r.Item,
                            r.Item.Header.icon,
                            r.Amount,
                            owned
                        );
                    })
                    .ToList();
            }


            // машина или модули
            VehicleEquipmentModule module = null;
            VehicleModule vehicle = null;
            if (item.HasModule<VehicleEquipmentModule>())
                module = item.VehicleEquipment;
            else
                vehicle = item.Vehicle;
            
            
            
            // === DESCRIPTER STATS
            List<StatDtoBase> descriptorDto = new();
            DescriptorStyleEntry descriptorEntry;
            var rawDescripter = item.GetDescriptors();
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

            var rawStats = item.GetStats();
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
            
            // *** добавляем финальный список настроеных статов
            statGroup.Add(new StatGroupViewDto(
                "Main",
                "Stats",
                statDto));
            
            

            bool purchased = _runtime.IsModuleUnlocked(moduleId);
            bool equipped = _runtime.GetCurrentVehicleId() == moduleId;

            return new GarageModuleDetailsDTO(
                moduleId,
                item.Header.titleLid,
                item.Header.icon,
                item.Classification.rarity,
                module != null ? module.Settings.slotType : VehicleSlotType.None,
                descriptorDto,
                statGroup,
                item.PrefabPath,
                item.PreviewConfig,
                requirements,
                purchased,
                equipped
            );
        }
        
        
        public IReadOnlyCollection<RuntimeId> GetUnlockedModules()
        {
            return _runtime.GetUnlockedModules();
        }

        public RuntimeId GetCurrentVehicleId()
        {
            return _runtime.GetCurrentVehicleId();
        }

        public bool TryPurchaseModule()
        {
            if (!GameContent.Items.TryGet(_currentModule, out var item))
                return false;

            
            // === проверяем рецепт
            var recipes= item.Recipes;
            if (recipes == null || recipes.Count <= 0)
                return false;

            var requirement = item.Recipes[0].Requirement;
            
            // === check
            if (!_requirementService.HasResources(requirement))
                return false;

            // === списание ресурсов (после проверки)
            foreach (var r in requirement)
                _inventory.TrySpend(r.Item.Id, r.Amount);

            _runtime.UnlockModule(_currentModule);
            
            return true;
        }
        

        public bool ReplaceVehicle()
        {
            // *** пушим событие если новый карго меньше текущего
            if (!GameContent.Items.TryGet(_currentModule, out var vehicleItem))
                return false;
            
            if (_gameLoopContext.PlayerTransport.HasOverflow(vehicleItem.Vehicle.CargoCapacity))
            {
                OnApplyFailed?.Invoke();
                return false;
            }
                
            _runtime.ReplaceCurrentVehicle(_currentModule);
            return true;
        }
    }
}