using System.Collections.Generic;
using Galactic1.Code.Gameplay.Weapons.Services;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Utils;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    public class InventoryView : MonoBehaviour
    {
        [Header("Dynamic")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private ScrollRect scrollRect;
        
        [Header("Static")]
        [SerializeField] private Transform[] slotRoot;
        
        [Space]
        [SerializeField] private TooltipInventoryUI tooltip;
        [SerializeField] private TMP_Text h1Text;
        [SerializeField] private TMP_Text unitNameText, unitLevelText;
        [SerializeField] private RawImage modelRender;
        [SerializeField] private GameObject hungryLabel, thirstLabel;

        [Header("Weapon")] 
        [SerializeField] private GameObject ammoWeaponRoot;
        [SerializeField] private GameObject weaponReloadButton;
        [SerializeField] private GameObject weaponUnloadButton;
        

        public RawImage ModelRender => modelRender;

        public InventoryManagementWindow Window { get; private set; }
        

        public IInventorySource _source { get; private set; }
        public InventoryAccessService _access { get; private set; }
        private WeaponReloadService _reloadService;
        private UIStyleResolver styleResolver;
        private ISlotViewProvider slotProvider;
        

        private bool isRaidMode;
        
        private List<InventorySlotView> slotsUI = new();
        public InventorySlotView selectedSlot { get; private set; }
        private int selectedWeaponIndex = -1;

        
        // =========================================================
        // CORE
        // =========================================================

        public void Bind(
            GameLoopContext gameLoopContext,
            InventoryManagementWindow window, 
            IInventorySource source, 
            InventoryAccessService access,
            WeaponReloadService weaponReloadService)
        {
            Window = window;
            
            _source = source;
            _access = access;
            _reloadService = weaponReloadService;
            styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();

            isRaidMode = _source != null
                ? _source.Owner is RaidUnitRuntime
                : false;
            

            // устанавливаем провайдер
            if(slotProvider == null)
            {
                if (scrollRect != null)
                    slotProvider = new DynamicSlotViewProvider(
                        gameObject.GetComponent<ExternalScrollbarBinder>(),
                        slotPrefab,
                        scrollRect);
                else
                    slotProvider = new StaticSlotViewProvider(slotRoot);
            }
            
            
            
            if (_source != null)
            {
                _source.OnChanged += RefreshUI;
                
                if (h1Text != null)
                    h1Text.text = GetNamePanel();

                // === вытаскиваем дату для вьюшки из дисплея UnitRuntime/RaidUnitRuntime
                if (source.Owner is IUnitRuntime runtime)
                {
                    var display = gameLoopContext.GetDisplayUnit(runtime.Id);
                    
                    if (unitNameText != null)
                        unitNameText.text = display.DisplayName;

                    if (unitLevelText != null)
                        unitLevelText.text = $"Level {display.Stats.Get(StatId.Level).Value}"; // <-- class Ranger ???

                    
                    if (hungryLabel != null)
                        hungryLabel.SetActive(display.Stats.Get(StatId.Hunger).Value <= 0);
                    
                    if (thirstLabel != null)
                        thirstLabel.SetActive(display.Stats.Get(StatId.Thirst).Value <= 0);
                }
            }
            else
            {
                if (h1Text != null) h1Text.text = "";
                if (unitNameText != null) unitNameText.text = "--";
                if (unitLevelText != null) unitLevelText.text = "--";
                
                if (hungryLabel != null) hungryLabel.SetActive(false);
                if (thirstLabel != null) thirstLabel.SetActive(false);
            }
            
            
            // === weapon ammo reload buttons
            if (!isRaidMode && ammoWeaponRoot != null)
            {
                // кнопки получают только левый источник инвентаря
                // потому что сейчас перезарядка оружия возможна только в слоте юнита
                // + юнит не имеет карманов, все кго слоты только для снаряжения, патроны туда не добавить !!!
                weaponReloadButton.RegisterButtonClick(() =>
                {
                    _reloadService.Reload(this, selectedWeaponIndex);
                    Window.UpdateButtons();
                });

                weaponUnloadButton.RegisterButtonClick(() =>
                {
                    _reloadService.Unload(this, selectedWeaponIndex);
                    Window.UpdateButtons();
                });
            }

            // сохраняем слоты UI
            SetupSlotView();

            RefreshUI();
            ClearSelection();
        }

        private void OnDisable()
        {
            if (_source != null)
                _source.OnChanged -= RefreshUI;
        }


        void SetupSlotView()
        {
            // slotsUI = new();
            // foreach (var sr in slotRoot)
            // {
            //     var l = sr.childCount;
            //     for (int i = 0; i < l; i++)
            //     {
            //         var view = sr.GetChild(i).GetComponent<InventorySlotView>();
            //         slotsUI.Add(view);
            //     }
            // }

            var slots = _source != null ? _access.GetSlots(_source) : null;
            var count = slots?.Count ?? 0;

            // провайдер сам решает: resize пула или вернуть статичный список
            slotsUI = new List<InventorySlotView>(slotProvider.GetSlots(count));
        }

        private void RefreshUI()
        {
            var l = slotsUI.Count;
            
            // === для пустых источников скрываем все слоты
            if (_source == null || _access?.GetSlots(_source).Count == 0)
            {
                for (int i = 0; i < l; i++)
                {
                    slotsUI[i].gameObject.CMP_Image().raycastTarget = false;
                    slotsUI[i].Empty();
                }
                return;
            }
            
            var slots = _access.GetSlots(_source);
            InventorySlotRuntime slotRuntime;
            
            for (int i = 0; i < l; i++)
            {
                slotsUI[i].gameObject.CMP_Image().raycastTarget = true;
                
                if (i < slots.Count)
                {
                    slotsUI[i].gameObject.CMP_Image().sprite = Window.spSlotEnable;
                    slotsUI[i].Init(this, i);
                    
                    slotRuntime = slots[i];
                    slotsUI[i].Set(new InventorySlotRuntime(
                        slotRuntime.Item,
                        slotRuntime.Amount,
                        slotRuntime.Durability,
                        slotRuntime.AmmoInMagazine),
                        styleResolver);
                }
                else
                {
                    slotsUI[i].gameObject.CMP_Image().sprite = Window.spSlotDisable;
                    slotsUI[i].Hide();
                }
            }
            
            UpdateButtons();
        }

        public InventorySlotRuntime GetSlot(int index)
        {
            // var slotProxy = _access.GetSlots(_source)[index];
            // return new(
            //     slotProxy.Item,
            //     slotProxy.Amount,
            //     slotProxy.Durability);
            return _access.GetSlot(_source, index);
        }

        public void SelectSlot(InventorySlotView slotView)
        {
            if (selectedSlot != null)
                selectedSlot.SetHighlight(false);

            selectedSlot = slotView;
            selectedSlot.SetHighlight(true);

            if (slotView.SlotIndex == 0)
                selectedWeaponIndex = slotView.SlotIndex;
            UpdateButtons();

            // 🔹 Обновляем кнопки при выборе
            Window.UpdateButtons();
        }

        /// <summary>
        /// Сброс выбранного слота и кнопок
        /// </summary>
        public void ClearSelection()
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetHighlight(false);
                selectedSlot = null;
                
                selectedWeaponIndex = -1;
                UpdateButtons();

                // 🔹 Обновляем кнопки при выборе
                Window.UpdateButtons();
            }
        }


        /// <summary>
        /// Подсвечивает подходящие слоты экипировки для предмета
        /// </summary>
        public void HighlightEquipmentSlots(ItemConfig item, bool highlight)
        {
            var access = Window.controller.AccessService;
            var rightSource = Window.controller.RightSource;

            if (rightSource == null || 
                !access._inventoryRules.IsEquipmentSource(rightSource))
                return;
            
            var slots = access.GetEquipmentSlots(rightSource);
            
            // ** сброс всех слотов
            if (item == null || 
                !access._equipmentValidation.CheckSource(rightSource, item))
            {
                foreach (var kvp in slots)
                    Window.rightSide.slotsUI[kvp.Key].SetHighlight(false);
            
                return;
            }
            
            
            var equipType = item.GetEquipSlot();
            
            foreach (var kvp in slots)
            {
                int index = kvp.Key;
                var slotType = kvp.Value;
            
                bool allowed = InventoryRules.IsEquipTypeAllowedForSlot(equipType, slotType);
            
                var slotUI = Window.rightSide.slotsUI[index];
                if (slotUI != null)
                    slotUI.SetHighlight(highlight && allowed);
            }
        }
        
        
        
        public void UpdateButtons()
        {
            if (ammoWeaponRoot == null) 
                return;
            
            // в рейде скрываем весь блок
            if (isRaidMode)
            {
                ammoWeaponRoot.SetActive(false);
                return;
            }
            
            // === меняем цвет через шейдер и блокируем картинку
            var _enable =
                selectedWeaponIndex != -1 &&
                !GetSlot(selectedWeaponIndex).IsEmpty;
            
            weaponReloadButton.CMP_Image().raycastTarget = _enable;
            weaponUnloadButton.CMP_Image().raycastTarget = _enable;
            ShaderPropertyUtil.SetFlash(
                weaponReloadButton.GetChild(0).CMP_Image(),
                Color.black, 
                _enable ? 0 : .5f);
            ShaderPropertyUtil.SetFlash(
                weaponUnloadButton.GetChild(0).CMP_Image(),
                Color.black, 
                _enable ? 0 : .5f);
            
            // восстанавливаем выбранный слот оружия
            if(selectedWeaponIndex != -1)
                selectedSlot.SetHighlight(true);
        }


        string GetNamePanel()
        {
            return _source.Type switch
            {
                InventorySourceType.BaseStorage => "Base Storage",
                InventorySourceType.TransportCargo => "Transport Cargo",
                InventorySourceType.WorldMapDrone => "Drone Cargo",
                InventorySourceType.LootContainer => "Loot",
                InventorySourceType.WorldContainer => "Crate",
                InventorySourceType.CorpseContainer => "Corpse",
                _ => ""
            };
        }
    }
}