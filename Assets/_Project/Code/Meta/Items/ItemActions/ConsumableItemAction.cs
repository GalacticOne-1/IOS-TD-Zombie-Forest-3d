
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Items
{
    [CreateAssetMenu(fileName = "ConsumableItemAction", menuName = "Game Configs/Inventory/Consumable Item Action")]
    public class ConsumableItemAction : ItemActionConfig
    {

        // public override void Execute(ItemContext ctx)
        // {
        //     if (!ctx.slot.Item.HasModule<UseModule>() ||
        //         !ctx.slot.Item.Use.ConsumeOnUse)
        //         return;
        //     
        //     IStatsController statsController =
        //         ctx.inventory is PlayerInventoryData || ctx.inventory is PlayerEquipmentInventoryData
        //             ? ServiceLocator.Current.Get<PlayerRepository>().GetController.StatsController
        //             : ServiceLocator.Current.Get<DragonRepository>().GetController.StatsController;
        //     
        //     
        //     var effects = ctx.slot.Item.Value.Config.ConsumableData.effects;
        //     
        //     
        //     foreach (var e in effects)
        //     {
        //         switch (e.key)
        //         {
        //             case EffectType.Hunger:
        //                 statsController.ModifyStat(StatType.Hunger, e.value);
        //                 DLog.Alert($"Hunger {e.value:+0;-0}");
        //                 break;
        //     
        //             case EffectType.Thirst:
        //                 statsController.ModifyStat(StatType.Thirst, e.value);
        //                 DLog.Alert($"Thirst {e.value:+0;-0}");
        //                 break;
        //     
        //             case EffectType.Heal:
        //                 statsController.ModifyStat(StatType.Health, e.value);
        //                 DLog.Alert($"Heal {e.value:+0;-0}");
        //                 break;
        //             
        //             case EffectType.Experience:
        //                 statsController.ModifyStat(StatType.Experience, e.value);
        //                 DLog.Alert($"Experience {e.value:+0;-0}");
        //                 break;
        //     
        //             case EffectType.BuffSpeed:
        //                 statsController.ModifyStat(StatType.MoveSpeed, e.value);
        //                 DLog.Alert($"Buff Speed {e.value:+0;-0}");
        //                 break;
        //     
        //             // новые кейсы добавляем здесь
        //         }
        //     }
        //     
        //     // floating text
        //     Vector3? slotPosition = ctx.ui?.selectedSlot.gameObject.CMP_RectTr().position;
        //     if (slotPosition.HasValue)
        //         ServiceLocator.Current.Get<FloatingTextService>().ShowText(
        //             slotPosition.Value, 
        //             $"-1 {ctx.slot.Item.Value.Header.TitleLid}", 
        //             Color.white);
        //     
        //     // ⚡ после применения можно уменьшить количество в стеке
        //     ctx.slot.Amount.Value--;
        //     if (ctx.slot.Amount.Value <= 0)
        //         ctx.slot.Clear();
        //     
        //     ctx.inventory.OnChanged?.Invoke();
        //     if (!ctx.slot.IsEmpty)
        //         ctx.ui?.selectedSlot.SetHighlight(true);
        //     else
        //         ctx.ui?.ClearSelection();
        // }

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
                UseModule = useModule
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