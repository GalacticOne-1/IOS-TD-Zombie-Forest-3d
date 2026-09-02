
using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Weapons.Services;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.RaidReport.Drone;
using Galactic1.Configs;
using Galactic1.Configs.UI;
using Galactic1.Core;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Items;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;
using Galactic1.UI.CharacterPreview;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public class InventoryManagementWindow : UIScreenPanel, IGameService
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private DragIcon dragIconPrefab;
        [SerializeField] private GameObject bClose;
        [SerializeField] private InventoryButtonRulesConfig buttonRulesConfig;
        [SerializeField] private CInventoryButtons _inventoryButtons;


        [Serializable]
        public struct CInventoryButtons
        {
            public GameObject bUse;
            public GameObject bSplit;
            public GameObject bSplitOne;
            public GameObject bSort;
            public GameObject bRemove;
            public GameObject bStoreAll;
            public GameObject bTakeAll;
        }

        public InventoryManagementController modeController { get; private set; }
        public InventoryController controller { get; private set; }
        public InventoryTransferSystem transferSystem { get; private set; }
        public InventoryManagementPanelState managementPanelState { get; private set; }
        public TooltipInventoryUI tooltip;
        
        private DragManager dragManager;
        public UICharacterPreview characterPreview { get; private set; }
        private UnitScrollListPresenter unitListPresenter;
        private GameSession _gameSession;
        
        

        [Serializable]
        public class UIElementRule
        {
            public GameObject element;
            public Func<bool> shouldBeActive; // функция, которая возвращает true если объект активен
        }

        private List<UIElementRule> buttonRules = new();
        [field: SerializeField] public Sprite spSlotEnable { get; private set; }
        [field: SerializeField] public Sprite spSlotDisable { get; private set; }


        [Header("🔹 UI Панели")] [SerializeField]
        private GameObject inventoryRoot;

        [SerializeField] private InventoryView leftInventoryView;
        [SerializeField] private InventoryView rightInventoryView;
        [SerializeField] private InventoryView rightEquipmentView;
        [SerializeField] private InventoryView rightDroneView;

        private DroneTabView _droneTabView;
        private DroneOpenContext _pendingDroneContext;

        // Ключ = (Тип инвентаря, Контекст), чтобы 1 тип мог иметь несколько UI
        private readonly Dictionary<(InventorySourceType, InventoryViewContext), InventoryView> uiConfigs = new();
        
        private List<UIButtonVisualRule> visualRules = new();

        public InventoryView leftSide { get; private set; }
        public InventoryView rightSide { get; private set; }
        
        public enum InventoryViewContext
        {
            Default,
            Crate,
            Equipment
        }
        
        
        
        


        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Register(this);
            canvas = container.Resolve<UIRootView>().canvas;
            
            ServiceLocator.Current.Get<TabPanelController>()
                .RegisterTab(new TabPanelController.RegistryEntry()
                    {
                        Label = "inventory",
                        Panel = this,
                        PanelId = UIScreenId.Inventory,
                        Order = 10
                    });

            _gameSession = container.Resolve<GameSession>();
            var gameplayContextService = new InventoryGameplayContextService(
                container,
                _gameSession);
            var accessService = new InventoryAccessService(gameplayContextService, this);
            
            transferSystem = new InventoryTransferSystem(
                this, 
                accessService);
            controller = new InventoryController(
                this, 
                transferSystem,
                accessService, 
                gameplayContextService,
                _gameSession.GameLoopContext);
            
            
            dragManager = new DragManager(
                canvas, 
                dragIconPrefab, 
                this, 
                tooltip);
            
            // смена источников инвентаря base/logistic/squad
            modeController = new InventoryManagementController(
                this,
                controller,
                transferSystem,
                gameplayContextService,
                _gameSession.GameLoopContext,
                _gameSession.StrategicSquadSystem,
                container.Resolve<UnitSceneLifecycleSystem>());
            
            
            // cargo drone
            _droneTabView = rightDroneView.GetComponent<DroneTabView>();
            _droneTabView.Initialize(
                container.Resolve<IEconomyService>(),
                ServiceLocator.Current.Get<UIStyleResolver>());
            
            
            // просто кнопки для контроллера источников
            managementPanelState = GetComponent<InventoryManagementPanelState>();
            managementPanelState.Initialize(modeController);

            // список юнитов
            unitListPresenter = GetComponent<UnitScrollListPresenter>();
            unitListPresenter.Initialize(modeController);
            _gameSession.UnitStatsRegister(modeController);
            
            // === preview unit
            characterPreview = ServiceLocator.Current.Get<UICharacterPreview>();
            modeController.OnUnitChanged += UpdatePreviewModel;
            accessService.OnPreviewUpdated += () => UpdatePreviewModel(modeController.SelectedUnit.unitId);
            //
            

            // buttons
            bClose.RegisterButtonClick(OnHide);
            //typeButtons[0].RegisterButtonClick(OpenCampMode);
            //typeButtons[1].RegisterButtonClick(OpenSquadMode);
            //typeButtons[2].RegisterButtonClick(OpenLogisticsMode);
            ButtonRules();
            SetupButtonVisualRules();

            // Регистрируем панели с контекстами
            uiConfigs[(InventorySourceType.BaseStorage, InventoryViewContext.Default)] = leftInventoryView;
            uiConfigs[(InventorySourceType.TransportCargo, InventoryViewContext.Default)] = leftInventoryView;
            uiConfigs[(InventorySourceType.TransportCargo, InventoryViewContext.Crate)] = rightInventoryView;
            uiConfigs[(InventorySourceType.UnitEquipment, InventoryViewContext.Equipment)] = rightEquipmentView;
            

            // uiConfigs[(typeof(DragonInventoryData), InventoryUIContext.Default)] = dragonUI;
            // uiConfigs[(typeof(DragonInventoryData), InventoryUIContext.Crate)] = dragonCrateUI;
            // uiConfigs[(typeof(DragonEquipmentInventoryData), InventoryUIContext.Equipment)] = dragonEquipmentUI;

            uiConfigs[(InventorySourceType.WorldMapDrone, InventoryViewContext.Crate)] = rightDroneView;
            uiConfigs[(InventorySourceType.LootContainer, InventoryViewContext.Crate)] = rightInventoryView;
            
            HideAll();
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<InventoryManagementWindow>();
        }
        
        

        private void Update()
        {
            dragManager?.Update();
        }
        
        public DragManager Drag => dragManager;
        
        
        
        
        /// <summary>
        /// Устанавливает контекст дрона перед Open().
        /// Вызывается из RaidReportFlowController до открытия окна.
        /// </summary>
        public void SetDroneContext(DroneOpenContext context)
        {
            _pendingDroneContext = context;
        }
        

        /// <summary>
        /// Открывает окно инвентаря для игрока и цели
        /// </summary>
        public void Open(IInventorySource leftSource, IInventorySource rightSource)
        {
            //transferSystem.OpenTransfer(leftSource, rightSource);
            HideAll();
            
            
            // регистрируем визуал модель
            //characterPreview.Show(null); 
            
            leftSide = null;
            rightSide = null;
            leftSide = GetViewForSource(leftSource);
            rightSide = GetViewForSource(rightSource);

            BindView(leftSource);
            BindView(rightSource);
            
            // === cargo drone
            if (rightSource != null && 
                rightSource.Type == InventorySourceType.WorldMapDrone
                && _pendingDroneContext != null)
            {
                _droneTabView.Show(
                    _pendingDroneContext.State,
                    rightSource,
                    _pendingDroneContext.OnSent);

                _pendingDroneContext = null; // сбрасываем после использования
            }
            //
            
            // обновление снаряги превью выбранного юнита
            // if (rightSource != null && 
            //     rightSource.Type == InventorySourceType.UnitEquipment)
            // {
            //     rightSource.OnChanged += () => UpdatePreviewModel(modeController.SelectedUnit.unitId);
            // }

            StateMenuButtons();
            UpdateButtons();
            inventoryRoot.SetActive(true);
            leftSide?.gameObject.SetActive(true);
            rightSide?.gameObject.SetActive(true);
        }

        /// <summary>
        /// Получает UI по типу и контексту (контекст определяется автоматически)
        /// </summary>
        private InventoryView GetViewForSource(IInventorySource source)
        {
            // === если источника нет вернем для снаряжения
            if (source == null &&
                uiConfigs.TryGetValue((
                        InventorySourceType.UnitEquipment,
                        InventoryViewContext.Equipment),
                    out var v))
                return v;
            
            var role = source.Type;
            var context = DetectContext(source);

            if (uiConfigs.TryGetValue((role, context), out var inventoryView))
                return inventoryView;

            // если контекстный вариант не найден — пробуем дефолт
            if (uiConfigs.TryGetValue((role, InventoryViewContext.Default), out var fallback))
                return fallback;

            Debug.LogError($"❌ Нет View для типа {role} (контекст {context})");
            return null;
        }

        /// <summary>
        /// Определяет контекст UI по типу контейнера
        /// </summary>
        private InventoryViewContext DetectContext(IInventorySource source)
        {
            if (source.Type == InventorySourceType.UnitEquipment ||
                source.Type == InventorySourceType.TransportEquipment)
                return InventoryViewContext.Equipment;
            
            if (source.Type == InventorySourceType.WorldMapDrone ||
                source.Type == InventorySourceType.LootContainer)
                return InventoryViewContext.Crate;

            // отсек транспорта справа если используем с базой
            if (source.Type == InventorySourceType.TransportCargo &&
                modeController.LeftSource.Type == InventorySourceType.BaseStorage)
                return InventoryViewContext.Crate;

            return InventoryViewContext.Default;
        }

        /// <summary>
        /// Привязывает контейнер к нужному UI (автоматически определяет контекст)
        /// </summary>
        private void BindView(IInventorySource source)
        {
            var view = GetViewForSource(source);
            if (view != null)
                view.Bind(
                    controller.GameLoopContext, 
                    this, 
                    source, 
                    controller.AccessService, 
                    controller.WeaponReloadService);
            else
                Debug.LogWarning($"❌ Нет View для {source.GetType().Name}");
        }


        /// <summary>
        /// Скрывает все панели
        /// </summary>
        private void HideAll()
        {
            inventoryRoot.SetActive(false);
            foreach (var ui in uiConfigs.Values)
                ui.gameObject.SetActive(false);
        }

        public override void OnHide()
        {
            base.OnHide();
            characterPreview.Clear(null);
            HideAll();
            modeController.Close();
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
        }


        public void ClearAllSelections()
        {
            leftSide?.ClearSelection();
            rightSide?.ClearSelection();
        }




        #region BUTTONS

        // нижние кнопки управления
        void StateMenuButtons()
        {
            foreach (var rule in buttonRules)
            {
                if (rule.element != null)
                    rule.element.SetActive(rule.shouldBeActive());
            }
        }

        public void UpdateButtons()
        {
            foreach (var rule in visualRules)
            {
                bool enabled = rule.isEnabled(this);
                rule.button.ButtonSetInteractable(enabled);
            }
        }
        
        /// <summary>
        /// Для смены текста кнопки
        /// </summary>
        /// <param name="text"></param>
        public void SetDynamicButtonText(string text)
        {
            var btn = GetButtonByName("Use");
            if (btn != null)
                btn.GetChild(0).CMP_Text().text = text;
        }


        private void SetupButtonVisualRules()
        {
            var styleDatabase = ServiceLocator.Current.Get<ConfigProvider>().Get<UIStyleDatabase>();
            visualRules.Clear();
            foreach (var rule in buttonRulesConfig.rules)
            {
                var btn = GetButtonByName(rule.buttonName).CMP_Btn();
                if (btn == null)
                {
                    Debug.LogWarning($"Кнопка с именем {rule.buttonName} не найдена в UI");
                    continue;
                }

                btn.SetStyleConfig(styleDatabase.Get<ButtonStyleConfig>(btn.styleId));
                visualRules.Add(new UIButtonVisualRule
                {
                    button = btn.gameObject,
                    isEnabled = (w) => rule.isEnabledLogic.Evaluate(w),
                    isHighlighted = (w) => false
                });
            }
        }

        private GameObject GetButtonByName(string name)
        {
            return name switch
            {
                "Use" => _inventoryButtons.bUse,
                "Split" => _inventoryButtons.bSplit,
                "SplitOne" => _inventoryButtons.bSplitOne,
                "Sort" => _inventoryButtons.bSort,
                "Remove" => _inventoryButtons.bRemove,
                "StoreAll" => _inventoryButtons.bStoreAll,
                "TakeAll" => _inventoryButtons.bTakeAll,
                _ => null
            };
        }

        void ButtonRules()
        {
            // #1 events for buttons
            _inventoryButtons.bUse.RegisterButtonClick(() =>
            {
                var view = GetActiveView();
                var slot = view?.selectedSlot;
                if (slot == null) return;
                
                int slotIndex = slot.SlotIndex;
                var slots = controller.AccessService.GetSlots(view._source);
                var slotProxy = slots[slotIndex];
                var item = slotProxy.Item;
                
                if (item == null) 
                    return;

                // Создаём контекст
                ItemContext ctx = new ItemContext(
                    view._source,
                    slotProxy,
                    slotIndex,
                    this,
                    view);

                // Используем систему
                ItemUseSystem.UseItem(ctx);
            });
            _inventoryButtons.bSplit.RegisterButtonClick(() => { controller.SplitStack(GetActiveView()); });
            _inventoryButtons.bSplitOne.RegisterButtonClick(() => { controller.SplitStack(GetActiveView(), true); });
            _inventoryButtons.bSort.RegisterButtonClick(() => { controller.SortInventory(GetActiveView()); });
            _inventoryButtons.bRemove.RegisterButtonClick(() =>
            {
                var data = new ConfirmPopupData(
                    "Confirm Deletion",
                    "Are you sure you want to delete the item(s)?",
                    "Confirm",
                    onOk: () => { controller.RemoveItem(GetActiveView()); },
                    onClose: () => { Debug.Log("Игрок отменил выход"); }
                );

                ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPopup, data);
                
            });
            _inventoryButtons.bStoreAll.RegisterButtonClick(controller.MoveAllLeftToRight);
            _inventoryButtons.bTakeAll.RegisterButtonClick(controller.MoveAllRightToLeft);

            
            // #2 правила для отображения общих элементов в зависимости от панелей
            buttonRules = new List<UIElementRule>()
            {
                // new()  // включить !
                // {
                //     element = _inventoryButtons.renderModel,
                //     shouldBeActive = () => rightSide._source.Type is 
                //         InventorySourceType.UnitEquipment or InventorySourceType.TransportEquipment
                // },
                new()
                {
                    element = _inventoryButtons.bUse,
                    shouldBeActive = () => true
                },
                new()
                {
                    element = _inventoryButtons.bSplit,
                    shouldBeActive = () => true
                },
                new()
                {
                    element = _inventoryButtons.bSplitOne,
                    shouldBeActive = () => true
                },
                new()
                {
                    element = _inventoryButtons.bSort,
                    shouldBeActive = () => true
                },
                new()
                {
                    element = _inventoryButtons.bRemove,
                    shouldBeActive = () => true
                },

                // кнопки отображаем если инвентарь - инвентарь
                new()
                {
                    element = _inventoryButtons.bStoreAll,
                    shouldBeActive = () => rightSide._source != null && 
                                           rightSide._source.Type != InventorySourceType.UnitEquipment &&
                                           rightSide._source.Type != InventorySourceType.TransportEquipment &&
                                           rightSide._source.Type != InventorySourceType.WorldMapDrone
                },
                new()
                {
                    element = _inventoryButtons.bTakeAll,
                    shouldBeActive = () => rightSide._source != null && 
                                           rightSide._source.Type != InventorySourceType.UnitEquipment &&
                                           rightSide._source.Type != InventorySourceType.TransportEquipment &&
                                           rightSide._source.Type != InventorySourceType.WorldMapDrone
                },
            };
        }


        // Возвращает активную сторону (левую или правую)
        public InventoryView GetActiveView()
        {
            if (rightSide._source == null)
                return leftSide;
            
            // Приоритет — если в правом окне есть выделение, то оно активное
            if (rightSide != null && rightSide.selectedSlot != null)
                return rightSide;

            if (leftSide != null && leftSide.selectedSlot != null)
                return leftSide;

            // Если ни одно не выделено — просто возвращаем левое
            return leftSide ?? rightSide;
        }



        
        void UpdatePreviewModel(string unitId)
        {
            if(controller.RightSource == null || 
               controller.RightSource.Type != InventorySourceType.UnitEquipment)
            {
                if (rightSide != null)
                    characterPreview.Clear(rightSide.ModelRender);
                return;
            }
            
               IUnitRuntime unitRuntime = !_gameSession.GameLoopContext.IsRaidState
                ? _gameSession.GameLoopContext.GetUnit(unitId)
                : _gameSession.GameLoopContext.CurrentRaid.Squad.GetUnit(unitId);
            DLog.Alert("UpdatePreviewModel", EDlogColor.ORANGE);
            if(unitRuntime != null)
            {
                var identityConfig = ServiceLocator.Current.Get<ConfigProvider>().Get<UnitIdentityPoolConfig>();
                var survEntry = identityConfig.GetSurvivorEntry(unitRuntime.ArchetypeId);
                characterPreview.Show(
                    rightSide.ModelRender,
                    AppConstants.PATH_PLAYER + survEntry.prefabPath,
                    survEntry.variant.AppearanceId,
                    identityConfig.PreviewConfig,
                    unitRuntime);
            }
            else
                characterPreview.Clear(rightSide.ModelRender);
        }

        #endregion



        

    }
}