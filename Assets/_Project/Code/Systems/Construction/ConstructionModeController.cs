using System;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.Gameplay.Construction.States;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems;
using Galactic1.Code.UI.Construction;
using Galactic1.Configs;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Scene-level controller управляющий режимом строительства.
    ///
    /// Отвечает за:
    /// - активацию режима строительства
    /// - деактивацию режима
    /// - запуск placement pipeline
    ///
    /// НЕ знает:
    /// - UI
    /// - Runtime зданий
    /// - GameLoopContext
    ///
    /// Работает только с Scene системами.
    /// </summary>
    public class ConstructionModeController : MonoBehaviour, IGameService
    {
        [SerializeField] private ConstructionPlacementController placement;
        [SerializeField] private GridShaderRenderer gridShaderRenderer;
        [SerializeField] private ConstructionInputRouter inputRouter;
        

        private ConstructionStateFactory _stateFactory;
        private IConstructionState _currentState;
        private ConstructionObjectMenuController objectMenu;
        private ConstructionRepairService _repairService;
        private CameraController _cameraControllerController;

        private bool _isActive;
        public bool IsActive => _isActive;

        public ConstructionPlacementController Placement => placement;

        public GridShaderRenderer GridShaderRenderer => gridShaderRenderer;

        public ConstructionObjectMenuController ObjectMenu => objectMenu;
        public CameraController CameraController => _cameraControllerController;

        public ConstructionContext Context { get; private set; } = new();

        public bool IsPlacingGhost => Context.HasGhost;
        public bool HasSelection => Context.HasSelection;



        public event Action OnStateChanged;
        
        
        

        public void Initialize(
            ConstructionStateFactory factory,
            ConstructionService constructionService,
            ConstructionRequirementService requirementService,
            ConstructionRepairService repairService,
            UIManager uiManager)
        {
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            _stateFactory = factory;
            _cameraControllerController = ServiceLocator.Current.Get<CameraController>();
            _repairService = repairService;

            objectMenu = "Prefabs/UI/Gameplay/Facilities/ObjectMenuController"
                .CreateGO(uiManager.TransformRoot.constructionRoot)
                .GetComponent<ConstructionObjectMenuController>();
            objectMenu.Initialize(_cameraControllerController.Camera);
            
            objectMenu.OnSwitchMovePressed += SwitchMove;
            objectMenu.OnRotatePressed += Rotation;
            objectMenu.OnCancelPressed += Cancel;
            objectMenu.OnConfirmPressed += Confirm;
            objectMenu.OnDeletePressed += Delete;
            objectMenu.OnRepairPressed += Repair;
            
            SetState(ConstructionStateType.Idle);

            var gridConfig = configProvider.Get<GridSettingsConfig>();
            
            placement.Initialize(
                this, 
                constructionService,
                requirementService,
                gridConfig,
                _cameraControllerController);
            
            gridShaderRenderer.CreateGrid(gridConfig);
            gridShaderRenderer.HideGrid();
            
            inputRouter.Initialize(
                this, 
                _cameraControllerController.Camera, 
                ServiceLocator.Current.Get<UIDetector>());
            
        }

        public void EnterMode()
        {
            if (_isActive) return;

            _isActive = true;
            gridShaderRenderer.ShowGrid();
            gridShaderRenderer.Reset(placement.ConstructionService.Grid); // todo перенести в move режим ?
            Context.Reset();
            SetState(ConstructionStateType.Idle);
        }

        public void ExitMode()
        {
            if (!_isActive) return;

            _isActive = false;
            
            gridShaderRenderer.HideGrid();
            placement.DestroyGhost();
            Context.Reset();
            HideObjectMenu();
            
            _currentState?.Exit();
            
            ServiceLocator.Current.Get<GameSession>().SaveIfDirty();
        }

        public void SetState(ConstructionStateType type)
        {
            _currentState?.Exit();
            _currentState = _stateFactory.Get(type);
            _currentState.Enter();
        }

        public void ResetState()
        {
            SetState(ConstructionStateType.Idle);
            FinishPlacement();
        }

        
        // стартовые здания не могут выбиратся в режиме стройки
        public bool CanFasilitySelect(BuildableObject buildable)
        {
            var id = buildable.FacilityConfig.Item.Id;

            if (id == GameIdProvider.Tavern ||
                id == GameIdProvider.Garage ||
                id == GameIdProvider.MainContainer) 
                return false;

            return true;
        }


        // =================
        // INPUT
        // =================

        public void OnCellClicked(Vector2Int cell)
        {
            _currentState?.OnCellClicked(cell);
        }

        public void OnObjectClicked(BuildableObject obj)
        {
            if (CanFasilitySelect(obj))
                _currentState?.OnObjectClicked(obj);
        }

        public void OnEmptyClicked()
        {
            if (Context.HasSelection)
            {
                Context.ClearSelection();
                HideObjectMenu();
                SetState(ConstructionStateType.Idle);
            }
        }

        public void ConstructionCompleted()
        {
            OnStateChanged?.Invoke();
        }

        public void Confirm()
        {
            _currentState?.OnConfirm();
        }

        public void Cancel()
        {
            _currentState?.OnCancel();
        }
        
        public void SwitchMove()
        {
            _currentState?.OnMove();
        }

        public void Rotation()
        {
            _currentState?.OnRotation();
        }

        #region Repair

        public void Repair()
        {
            _currentState?.OnRepair();
        }

        public void RefreshRepairUI(RepairRequirementResult result)
        {
            objectMenu.RefreshRepair(result);
        }

        public void ShowRepairAlert(string message)
        {
            objectMenu.ShowAlert(!string.IsNullOrEmpty(message), message ?? string.Empty);
        }

        #endregion

        public void Delete()
        {
            var data = new ConfirmPopupData(
                "Confirm Deletion",
                "Are you sure you want to delete the item(s)?",
                "Confirm",
                onOk: () =>
                {
                    _currentState?.OnDelete();
                    OnStateChanged?.Invoke();
                },
                onClose: () => {  }
            );

            ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPopup, data);
        }

        // =================
        // BUILD CARD
        // =================

        public void StartPlacement(FacilityModule config)
        {
            Context.BuildConfig = config;
            SetState(ConstructionStateType.PlacingGhost);
        }
        
        public void FinishPlacement()
        {
            gridShaderRenderer.Reset(placement.ConstructionService.Grid);
            ResetPlacementUI();
        }
        
        // =================
        // OBJECT MENU
        // =================

        public void ShowObjectMenu(BuildableObject obj, EConstructionSubMenu menu)
        {
            objectMenu.Show(obj, menu);
        }

        public void HideObjectMenu()
        {
            objectMenu.Hide();
        }
        
        /// <summary>
        /// Обновляет UI состояние валидности placement.
        /// Используется и для ghost и для перемещения.
        /// </summary>
        public void UpdatePlacementState(PlacementValidationResult result)
        {
            objectMenu.SetConfirmButtonEnabled(result.IsValid);
            objectMenu.ShowAlert(!result.IsValid, result.Message);
        }
        
        /// <summary>
        /// Сброс UI состояния placement
        /// </summary>
        public void ResetPlacementUI()
        {
            objectMenu.SetConfirmButtonEnabled(false);
            objectMenu.ShowAlert(false, "");
        }
    }
}