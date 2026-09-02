using Galactic1.Code.UI.Construction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Перемещение существующего объекта
    /// </summary>
    public class MovingObjectState : ConstructionStateBase
    {
        public MovingObjectState(
            ConstructionModeController controller,
            ConstructionPlacementController placement)
            : base(controller, placement)
        {
        }

        public override void Enter()
        {
            var obj = controller.Context.SelectedObject;
            placement.StartMove(obj);
            controller.ShowObjectMenu(obj, EConstructionSubMenu.Move);
        }

        public override void Exit()
        {
            base.Exit();
            // Если был объект в режиме перемещения — отменяем move
            placement.CancelMove();
        }

        public override void OnCellClicked(Vector2Int cell)
        {
            placement.MoveTo(cell);
        }
        
        // public override void OnRotation()
        // {
        //     var obj = controller.Context.SelectedObject;
        //
        //     if (obj == null)
        //         return;
        //
        //     int rotation = obj.FootprintRuntime.Rotation + 1;
        //
        //     if (rotation >= 4)
        //         rotation = 0;
        //
        //     placement.ApplyRotation(rotation);
        // }

        public override void OnRotation()
        {
            var obj = controller.Context.SelectedObject;

            if (obj == null)
                return;

            var oldFootprint = obj.FootprintRuntime.Footprint.Rotate(obj.FootprintRuntime.Rotation);
            var oldOrigin = obj.FootprintRuntime.Origin;

            int rotation = (obj.FootprintRuntime.Rotation + 1) % 4;

            placement.ApplyRotation(rotation);

            var newFootprint = obj.FootprintRuntime.Footprint.Rotate(rotation);

            float centerX = oldOrigin.x + oldFootprint.Width * 0.5f;
            float centerY = oldOrigin.y + oldFootprint.Height * 0.5f;

            int newOriginX = Mathf.RoundToInt(centerX - newFootprint.Width * 0.5f);
            int newOriginY = Mathf.RoundToInt(centerY - newFootprint.Height * 0.5f);

            placement.MoveTo(new Vector2Int(newOriginX, newOriginY));
        }

        public override void OnConfirm()
        {
            placement.ConfirmMove();
            controller.SetState(ConstructionStateType.SelectedObject);
        }

        public override void OnCancel()
        {
            placement.CancelMove();
            controller.Context.ClearSelection();
            controller.SetState(ConstructionStateType.Idle);
        }
    }
}