using System;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public class AbilityComponent : IAbilityAnimationReceiver
    {
        private PendingAbilityExecution _pending;

        public event Action OnFinished;


        public void SetPending(
            ItemUseContext ctx,
            InventorySlotRuntime slot,
            ConsumableBehaviour behaviour)
        {
            _pending = new PendingAbilityExecution
            {
                Context = ctx,
                Slot = slot,
                Behaviour = behaviour
            };
        }
        
        public void ExecutePending()
        {
            if (_pending == null)
                return;

            var pending = _pending;
            _pending = null;

            pending.Behaviour.Execute(
                pending.Context,
                pending.Slot);
        }

        public void OnAbilityFinished() => OnFinished?.Invoke();
    }
}