
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Effect
{
    public sealed class ItemUseService
    {

        public void Use(ItemUseContext ctx)
        {
            var source = ctx.User.InventorySource.Equipment;
            var slot = ctx.User.QuickSlot.GetSlot(source, ctx.QuickSlotIndex);

            if (slot == null || slot.IsEmpty)
                return;

            if (!ctx.User.Cooldowns.IsReady(slot.Item.Id.Guid))
                return;

            var behaviour = slot.Item.Use?.Behaviour;

            if (behaviour == null)
                return;

            if (!behaviour.CanUse(ctx, slot))
                return;

            // =========================
            // 🔥 ВАЖНО: через анимацию
            // =========================
            if (behaviour is GrenadeBehaviour)
            {
                //_execution.SetPending(ctx, slot, behaviour);
                var repo = ServiceLocator.Current.Get<SurvivorRepository>().TryGet(ctx.User.Id);

                repo.instance.Ability.SetPending(
                    ctx,
                    slot,
                    behaviour);
                
                ctx.AnimationType = AbilityAnimationType.TossGrenade;
                ctx.User.RequestAbilityAnimation(ctx);

                return;
            }

            // =========================
            // instant fallback
            // =========================
            behaviour.Execute(ctx, slot);
        }

    }
}