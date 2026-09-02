using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.Units;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Application Controller экрана Inventory Management.
    /// Управляет режимами, выбранными юнитами, источниками инвентаря
    /// и координирует InventoryController / TransferSystem / GameplayContext.
    /// </summary>
    public sealed class InventoryManagementController
    {
        private readonly InventoryManagementWindow _window;
        private readonly InventoryController _inventoryController;
        private readonly InventoryTransferSystem _transferSystem;
        private readonly InventoryGameplayContextService _gameplayContext;
        private readonly StrategicSquadSystem _strategicSquadSystem;
        private readonly UnitSceneLifecycleSystem _unitSceneLifecycleSystem;
        private readonly GameLoopContext _gameLoopContext;

        private InventoryGameplayMode currentMode;
        private IInventorySource leftSource;
        private IInventorySource rightSource;
        
        
        
        private List<UnitDisplayData> visibleUnits = new();
        /// <summary> Units that UI should display in the scroll list. </summary>
        public IReadOnlyList<UnitDisplayData> VisibleUnits => visibleUnits;
        public (int viewIndex, string unitId) SelectedUnit { get; private set; }
        
        

        public InventoryGameplayMode CurrentMode => currentMode;
        public IInventorySource LeftSource => leftSource;
        public IInventorySource RightSource => rightSource;

        public event Action OnSourcesChanged;
        public event Action<string> OnUnitChanged;
        public event Action OnUnitListChanged;
        public event Action<int, string> OnSelectionChanged;
        
        
        public InventoryManagementController(
            InventoryManagementWindow window,
            InventoryController inventoryController,
            InventoryTransferSystem transferSystem,
            InventoryGameplayContextService gameplayContext,
            GameLoopContext gameLoopContext,
            StrategicSquadSystem strategicSquadSystem, 
            UnitSceneLifecycleSystem unitSceneLifecycleSystem)
        {
            _window = window;
            _inventoryController = inventoryController;
            _transferSystem = transferSystem;
            _gameplayContext = gameplayContext;
            _gameLoopContext = gameLoopContext;
            _strategicSquadSystem = strategicSquadSystem;
            _unitSceneLifecycleSystem = unitSceneLifecycleSystem;


            gameLoopContext.OnUnitDeletedByPlayer += OnOpen;  // для обновления панели после увольнения юнита
            strategicSquadSystem.OnSquadChanged += OnSquadChanged;
            
            // === отписка
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                gameLoopContext.OnUnitDeletedByPlayer -= OnOpen;
            }));
        }

        // =========================================================
        // OPEN / CLOSE
        // =========================================================

        public void Open(InventoryGameplayMode mode)
        {
            _window.managementPanelState.ResolvePanelState(InventoryPanelState.CampFull);
            
            
            // режим для отчета после рейда
            if (mode == InventoryGameplayMode.Transport_BufferLoot)
            {
                _window.managementPanelState.ResolvePanelState(InventoryPanelState.RaidReportLoot);
            }
            else if (mode == InventoryGameplayMode.Transport_BufferDrone)
            {
                _window.managementPanelState.ResolvePanelState(InventoryPanelState.RaidReportDrone);
            }
            
            // меняем режим для рейда или карты
            else if (_gameLoopContext.IsRaidState || _gameLoopContext.IsWorldMapState)
            {
                mode = InventoryGameplayMode.Transport_SquadOnly;
                _window.managementPanelState.ResolvePanelState(InventoryPanelState.RaidLocked);
            }
            
            
            currentMode = mode;

            // === получаем первый в списке юнит
            string unitId = "";
            if(mode == InventoryGameplayMode.Camp_AllUnits)
            {
                var units = _gameLoopContext.PlayerUnits.ToList();
                if (units.Count > 0)
                    unitId = units[0].Proxy.Id;
            }
            else if (mode == InventoryGameplayMode.Camp_SquadOnly || mode == InventoryGameplayMode.Transport_SquadOnly)
            {
                var units = _gameLoopContext.StrategicSquadUnits.ToList();
                if (units.Count > 0)
                    unitId = units[0].Proxy.Id;
            }


            SelectedUnit = (0, unitId);

            BuildSources();
            ApplySourcesToView();
            OnUnitChanged?.Invoke(SelectedUnit.unitId);
        }

        public void Close()
        {
            leftSource?.Dispose();
            rightSource?.Dispose();
            leftSource = null;
            rightSource = null;
        }

        // =========================================================
        // MODE LOGIC
        // =========================================================

        private void BuildSources()
        {
            var result = _gameplayContext.BuildMode(currentMode);

            leftSource?.Dispose();
            rightSource?.Dispose();
            leftSource = result.left;
            rightSource = result.right;
            
            BuildVisibleUnitList();
        }

        private void ApplySourcesToView()
        {
            _transferSystem.OpenTransfer(leftSource, rightSource);
            _window.Open(leftSource, rightSource);
            OnSourcesChanged?.Invoke();
        }

        // =========================================================
        // UNIT SELECTION
        // =========================================================
        
        /// <summary>
        /// Builds unit list for UI depending on gameplay mode.
        /// </summary>
        private void BuildVisibleUnitList()
        {
            visibleUnits.Clear();
            

            switch (currentMode)
            {
                case InventoryGameplayMode.Camp_AllUnits:
                    visibleUnits.AddRange(_gameLoopContext.GetDisplayAllUnit());
                    break;

                case InventoryGameplayMode.Camp_SquadOnly:
                case InventoryGameplayMode.Transport_SquadOnly:
                    visibleUnits.AddRange(_gameLoopContext.GetDisplaySquadUnit());
                    break;

                case InventoryGameplayMode.Camp_And_Transport:
                    // Logistics mode → no units
                    break;
            }

            foreach (var u in visibleUnits)
            {
                DLog.Alert("inv "+u.DisplayName);
            }
            //selectedUnitIndex = Math.Clamp(selectedUnitIndex, 0, visibleUnits.Count - 1);
            OnUnitListChanged?.Invoke();
        }

        /// <summary>
        /// Выбирает юнит по индексу в visibleUnits и обновляет его инвентарь справа.
        /// </summary>
        public void SelectUnitByIndex(int viewIndex, string unitId)
        {
            if (viewIndex < 0 || viewIndex >= visibleUnits.Count)
                return;

            SelectedUnit = (viewIndex, unitId);
            OnSelectionChanged?.Invoke(viewIndex, unitId);
            RefreshUnit();
        }

        private void RefreshUnit()
        {
            if (visibleUnits.Count == 0)
                return;

            var display = visibleUnits[SelectedUnit.viewIndex];
            
            // ❗ получаем UnitRuntime по Id
            var unitRuntime = _gameLoopContext.GetUnit(display.Id);

            var unitSource = _gameplayContext.GetUnitInventorySource(unitRuntime, currentMode);

            if (unitSource == null)
                return;

            rightSource?.Dispose();
            rightSource = unitSource;

            ApplySourcesToView();
            OnUnitChanged?.Invoke(SelectedUnit.unitId);
        }


        
        
        void OnOpen(string unitId)
        {
            Open(currentMode);
        }

        void OnSquadChanged(string unitId, bool isInSquad)
        {
            if (currentMode != InventoryGameplayMode.Camp_AllUnits)
                Open(currentMode);
        }


        // ------------ Helpers -------------
        public bool SquadMode()
            => currentMode switch
            {
                InventoryGameplayMode.Camp_AllUnits
                    or InventoryGameplayMode.Camp_SquadOnly
                    or InventoryGameplayMode.Transport_SquadOnly => true,
                _ => false
            };
    }
}
