using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Systems.Raid;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Items
{
    [CreateAssetMenu(
        fileName = "ConsumableItemAction",
        menuName = "Game Configs/Inventory/Consumable Item Action")]
    public class ConsumableItemAction : ItemActionConfig
    {

        public override void Execute(ItemContext ctx)
        {
            if (!ctx.window.modeController.SquadMode())
                return;
            
            if (!ctx.slot.Item.HasModule<UseModule>())
                return;

            var useModule = ctx.slot.Item.Use;

            if (useModule?.Behaviour == null)
                return;

            // =========================
            // 🎯 Определяем юнита-цель
            // =========================
            IUnitRuntime user = ResolveUser(ctx);
            
            if (user == null)
                return;
            
            var runtimeCtx = new ItemUseContext
            {
                User = user,
                SceneUnit = null, // можно подтянуть через repository если нужно
                InventorySource = ctx.inventorySource,
                SlotIndex = ctx.slotIndex,
                QuickSlotIndex = -1,
                UseSmartTarget = false,
                SquadMembers = null,
                UseModule = useModule,
                OnFinished = () =>
                {
                    if (ctx.slot.Amount > 0)
                    {
                        ctx.view?.selectedSlot?.SetHighlight(true);
                    }
                    else
                    {
                        ctx.view?.ClearSelection();
                    }
                }
            };


            // использование предмета
            ctx.slot.Item.Use.Behaviour.Execute(
                runtimeCtx,
                ctx.slot, () =>
                {
                    // при успешном использовании предмета
                    
                    Vector3? slotPosition = ctx.view?.selectedSlot.gameObject.CMP_RectTr().position;
                    if (slotPosition.HasValue)
                        ServiceLocator.Current.Get<FloatingTextService>().ShowText(
                            slotPosition.Value,
                            $"-1 {ctx.slot.Item.Header.titleLid}",
                            Color.white);

                    if (!ctx.slot.IsEmpty)
                        ctx.view?.selectedSlot.SetHighlight(true);
                    else
                        ctx.view?.ClearSelection();
                });
        }

        private IUnitRuntime ResolveUser(ItemContext ctx)
        {
            var unitId = ctx.window.modeController.SelectedUnit.unitId;

            return ServiceLocator.Current.Get<GameSession>().GameLoopContext.GetUnitRuntime(unitId);
        }

    }
}