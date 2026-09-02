using Galactic1.Code.Cameras;
using Galactic1.Code.UI.Construction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Размещение нового объекта
    /// </summary>
    public class PlacingGhostState : ConstructionStateBase
    {
        private readonly CameraController _cameraController;

        public PlacingGhostState(
            ConstructionModeController controller,
            ConstructionPlacementController placement,
            CameraController cameraController)
            : base(controller, placement)
        {
            _cameraController = cameraController;
        }

        public override void Enter()
        {
            var ctx = controller.Context;
            
            placement.CreateGhost(ctx.BuildConfig);
            ctx.CurrentGhost = placement.CurrentGhost;
            
            // 1. Определяем стартовую клетку (центр экрана)
            if (_cameraController.ScreenCenterToBuildPlane(out Vector3 world, new Vector2(0, 0)))
            {
                //DebugScene.CreateSphere(world, Color.blue, .8f);
                
                Vector2Int centerCell = placement.ConstructionService.Coordinates.WorldToCell(world);

                // 2. Находим подходящую клетку с учётом размера
                Vector2Int startCell = placement.GetInitialPlacementCell(ctx.BuildConfig, centerCell);

                // 3. Ставим гост и красим сетку
                placement.MoveGhost(startCell);
            }
            
            controller.ShowObjectMenu(ctx.CurrentGhost, EConstructionSubMenu.Move);
        }

        public override void Exit()
        {
            base.Exit();
            placement.DestroyGhost();
            controller.Context.ClearBuild();
        }

        public override void OnCellClicked(Vector2Int cell)
        {
            placement.MoveGhost(cell);
        }

        // public override void OnRotation()
        // {
        //     var context = controller.Context;
        //     if (context.Preview == null)
        //         return;
        //
        //     context.Preview.Rotation++;
        //
        //     if (context.Preview.Rotation >= 4)
        //         context.Preview.Rotation = 0;
        //
        //     placement.GhostRotation(context.Preview.Rotation);
        // }
        public override void OnRotation()
        {
            var context = controller.Context;
            if (context.Preview == null)
                return;

            var oldFootprint = context.Preview.Footprint;
            var oldOrigin = context.Preview.Origin;

            context.Preview.Rotation = (context.Preview.Rotation + 1) % 4;

            placement.GhostRotation(context.Preview.Rotation);

            var newFootprint = context.Preview.Footprint;

            float centerX = oldOrigin.x + oldFootprint.Width * 0.5f;
            float centerY = oldOrigin.y + oldFootprint.Height * 0.5f;

            int newOriginX = Mathf.RoundToInt(centerX - newFootprint.Width * 0.5f);
            int newOriginY = Mathf.RoundToInt(centerY - newFootprint.Height * 0.5f);

            placement.MoveGhost(new Vector2Int(newOriginX, newOriginY));
        }
        

        public override void OnConfirm()
        {
            var ctx = controller.Context;

            placement.Build();

            if (ctx.BuildConfig.FootprintConfig.autoBuild)
            {
                placement.MoveGhostNextCell();
            }
            else
            {
                placement.DestroyGhost();
                controller.HideObjectMenu();
                ctx.ClearBuild();
                controller.SetState(ConstructionStateType.Idle);
            }
        }

        public override void OnCancel()
        {
            placement.DestroyGhost();
            controller.Context.ClearBuild();
            controller.SetState(ConstructionStateType.Idle);
        }
    }
}