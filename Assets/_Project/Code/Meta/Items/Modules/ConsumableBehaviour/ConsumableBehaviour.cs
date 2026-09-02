using System;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Inventory.Abstractions;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    public abstract class ConsumableBehaviour
    {
        public abstract ConsumableType Type { get; }

        public virtual bool SupportsSmartTarget => false;
        public virtual float DoubleTapWindow => 0.35f;
        
        public virtual bool IsImmediate => true;
        
        public virtual UseActivationType ActivationType => UseActivationType.Instant;
        public virtual AbilityAimType AimType => AbilityAimType.Circle;
        

        public abstract bool CanUse(
            ItemUseContext ctx, 
            InventorySlotRuntime slot);
        
        public abstract void Execute(
            ItemUseContext ctx, 
            InventorySlotRuntime slot, 
            Action onSuccess = null);
        
        
        /// <summary>
        /// Проверяет возможность использовать предмет по выбранной точке.
        /// По умолчанию — только проверка дальности.
        /// </summary>
        public virtual bool ValidateTarget(
            Vector3 origin,
            Vector3 target,
            UseModule config,
            out Vector3 projected)
        {
            projected = target;

            return Vector3.Distance(origin, target) <= config.Range;
        }
        
        
        /// <summary>
        /// Вычитаем из инвентаря
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="slot"></param>
        protected virtual void Consume(ItemUseContext ctx, InventorySlotRuntime slot)
        {
            slot.Amount--;
            var source = ctx.InventorySource;

            int index = ctx.SlotIndex;
            
            if (slot.Amount <= 0)
                source.ClearSlot(index);
            else
                source.SetSlot(index, slot);
            
            source.NotifyChanged();
        }
    }
}