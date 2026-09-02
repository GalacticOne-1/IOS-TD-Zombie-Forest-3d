using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Buildings;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;

namespace Galactic1.Code.UI.Garage
{
    /// <summary>
    /// Контроллер окна гаража.
    /// Только UI — читает DTO, команды отправляет через GarageSceneAdapter.
    /// </summary>
    public class GaragePanelModule : FacilityPanelModule
    {
        [SerializeField] private GarageMainPanelView garageMainPanelView;
        [SerializeField] private GarageModulesPanelView garageModulesPanelView;


        private UIModulePreview _preview;
        private GarageSceneAdapter _adapter;
        private GarageDetailsDTO _details;
        

        private VehicleSlotType _selectedCategory;
        
        
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.Garage;
        
        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null,
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);

            _preview ??= ServiceLocator.Current.Get<UIModulePreview>();
            _adapter = sceneAdapter as GarageSceneAdapter;

            _adapter.OnApplyFailed += HandleApplyFailed;
            garageModulesPanelView.OnModuleSelected += HandleModuleSelected;
            
            
            garageModulesPanelView.BindBuy(BuyModule);
            garageModulesPanelView.BindApply(ApplyModule);
            garageModulesPanelView.BindBack(ShowMainPanel);
            
            
            Rebind(dto);
            ShowMainPanel();
        }

        public override void Unbind()
        {
            base.Unbind();
            if (_adapter != null)
            {
                _adapter.OnApplyFailed -= HandleApplyFailed;
                garageModulesPanelView.OnModuleSelected -= HandleModuleSelected;
                garageModulesPanelView.Unbind();
            }

            _preview?.Clear(null);
            _details = null;
            _adapter = null;
        }

        public override void Rebind(FacilityDTO dto)
        {
            _details = dto.Details as GarageDetailsDTO;
            
        }
        
        
        // =================================================
        // PANEL STATES
        // =================================================

        private void ShowMainPanel()
        {
            garageMainPanelView.gameObject.SetActive(true);
            garageModulesPanelView.gameObject.SetActive(false);
            _adapter.SetCurrentModule(null);
            
            garageMainPanelView.Bind(
                _preview, 
                _details.PrefabPath, 
                _details.PreviewConfig, 
                GetSlotIcon,
                SelectCategory);
        }

        private void ShowModulesPanel()
        {
            garageMainPanelView.gameObject.SetActive(false);
            garageModulesPanelView.gameObject.SetActive(true);
        }

        // =================================================
        // CATEGORY
        // =================================================

        private void SelectCategory(VehicleSlotType category)
        {
            _selectedCategory = category;

            
            
            // возвращаем выбор модуля
            if (GameContent.Items.TryGet(_adapter.CurrentModule, out var item))
            {
                garageModulesPanelView.SelectCard(_adapter.CurrentModule);
            }
            else
            {
                // выбираем установленный или первый в списке
                var id = GetEquippedModule(_selectedCategory) ?? GetModules(_selectedCategory)[0].Id;
                garageModulesPanelView.Build(
                    _adapter.GetModuleDetails(id),
                    _preview,
                    GetModules(category),
                    GetUnlockedModules(),
                    GetEquippedModule(category));
                garageModulesPanelView.SelectCard(id);
            }

            ShowModulesPanel();
        }

        // =================================================
        // MODULE SELECT
        // =================================================
        
        private void HandleModuleSelected(RuntimeId moduleId)
        {
            _adapter.SetCurrentModule(moduleId);
            var dto = _adapter.GetModuleDetails(moduleId);

            garageModulesPanelView.SetModuleDetails(dto);
            
        }

        // =================================================
        // APPLY
        // =================================================
        
        private void BuyModule()
        {
            if (_adapter.TryPurchaseModule())
            {
                // обновляем UI
                SelectCategory(_selectedCategory);
            }
        }

        private void ApplyModule()
        {
            if (!_adapter.ReplaceVehicle())
                return;
            
            var moduleId = _adapter.CurrentModule;
            var dto = _adapter.GetModuleDetails(moduleId);
            
            garageMainPanelView.UpdateSlot(
                _selectedCategory,
                dto.Icon
            );
            
            garageModulesPanelView.UpdateEquipped(
                _adapter.GetCurrentVehicleId()
            );

            // Обновляем панель — новый equipped
            SelectCategory(_selectedCategory);
        }
        
        
        private void HandleApplyFailed()
        {
            ServiceLocator.Current.Get<UIManager>().OpenPopup(
                UIScreenId.AdAlertToast,
                "Inventory reduction, some items will be lost!");
        }

        // =================================================
        // DATA
        // =================================================
        
        private Sprite GetSlotIcon(VehicleSlotType category)
        {
            var moduleId = GetEquippedModule(category);
            if (moduleId == null)
                return null;

            var dto = _adapter.GetModuleDetails(moduleId);
            return dto.Icon;
        }

        private IReadOnlyList<ItemConfig> GetModules(VehicleSlotType category)
        {
            IReadOnlyList<ItemConfig> result = category switch
            {
                VehicleSlotType.None => _details?.AvailableVehicles,
            };

            if (result == null)
                result = Array.Empty<ItemConfig>();

            return result;
        }

        private IReadOnlyCollection<RuntimeId> GetUnlockedModules()
        {
            return _adapter?.GetUnlockedModules();
        }
        
        private RuntimeId GetEquippedModule(VehicleSlotType category)
        {
            return category switch
            {
                _ => _adapter?.GetCurrentVehicleId() // сейчас возвращаем только модуль машины
            };
        }
        
    }
}