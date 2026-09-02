using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.UI.Construction;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Выбран существующий объект
    /// </summary>
    public class SelectedObjectState : ConstructionStateBase
    {
        private readonly ConstructionRepairService _repairService;
        private IRepairableFacility _repairableFacility;

        public SelectedObjectState(
            ConstructionModeController controller,
            ConstructionPlacementController placement,
            ConstructionRepairService repairService)
            : base(controller, placement)
        {
            _repairService = repairService;
        }

        public override void Enter()
        {
            var obj = controller.Context.SelectedObject;

            if (obj == null)
                return;

            controller.ShowObjectMenu(obj, EConstructionSubMenu.Upgrade);
            controller.ResetPlacementUI();
            controller.CameraController.FocusOnPositionFacility(obj.transform.position);

            SubscribeToRuntime(obj);
            RefreshRepairUI();
        }

        public override void Exit()
        {
            base.Exit();
            UnsubscribeFromRuntime();
        }

        public override void OnObjectClicked(BuildableObject obj)
        {
            controller.Context.SelectedObject = obj;
            controller.SetState(ConstructionStateType.SelectedObject);
        }

        public override void OnCellClicked(Vector2Int cell)
        {
            controller.Context.ClearSelection();
            controller.SetState(ConstructionStateType.Idle);
        }

        public override void OnMove()
        {
            controller.SetState(ConstructionStateType.MovingObject);
        }

        public override void OnConfirm()
        {
        }

        public override void OnCancel()
        {
            controller.Context.ClearSelection();
            controller.SetState(ConstructionStateType.Idle);
        }

        public override void OnDelete()
        {
            var obj = controller.Context.SelectedObject;
            if (obj == null) return;

            placement.DeleteObject(obj);
            controller.Context.ClearSelection();
            controller.SetState(ConstructionStateType.Idle);
        }

        public override void OnRepair()
        {
            var obj = controller.Context.SelectedObject;
            if (obj == null)
                return;

            var result = _repairService.TryRepair(obj);

            if (!result.Success)
            {
                controller.ShowRepairAlert(GetFailMessage(result.FailReason));
                return;
            }

            // === успешная починка
            ServiceLocator.Current.Get<GameSession>().MarkDirty();
            controller.ShowRepairAlert(null);
            // RefreshRepairUI() будет дополнительно вызван через OnHealthChanged,
            // но вызываем сразу для мгновенного визуального отклика.
            RefreshRepairUI();
        }

        private void SubscribeToRuntime(BuildableObject obj)
        {
            _repairableFacility = obj.Adapter as IRepairableFacility;

            if (_repairableFacility != null)
                _repairableFacility.OnHealthChanged += OnHealthChanged;
        }

        private void UnsubscribeFromRuntime()
        {
            if (_repairableFacility != null)
                _repairableFacility.OnHealthChanged -= OnHealthChanged;

            _repairableFacility = null;
        }

        private void OnHealthChanged(float current, float max) => RefreshRepairUI();

        private void RefreshRepairUI()
        {
            var obj = controller.Context.SelectedObject;
            if (obj == null)
                return;

            var result = _repairService.GetRepairState(obj);
            controller.RefreshRepairUI(result);
        }

        private static string GetFailMessage(RepairFailReason reason) => reason switch
        {
            RepairFailReason.NotEnoughResources => "Not enough resources to repair",
            RepairFailReason.AlreadyFull => "Building is already fully repaired",
            RepairFailReason.NotRepairable => "This building cannot be repaired",
            _ => string.Empty
        };
    }
}