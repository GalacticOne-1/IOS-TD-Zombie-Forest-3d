using System;
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Raid;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Game.Meta.Items
{
    [Serializable]
    public sealed class HealBehaviour : ConsumableBehaviour
    {
        [FormerlySerializedAs("audioDefinition")] [SerializeField] private SimpleAudioConfig audioConfig;
        
        [Header("Targeting")] 
        public bool supportsSmartTarget = true;
        public float doubleTapWindow = 0.35f;

        [Header("Cast")] 
        public bool requiresCast = true;
        public float castTime = 1.5f;

        [Header("Effect")] 
        public float healAmount = 50f;
        public bool isInstant = false;
        public bool isHoT = true;
        public float hotDuration = 4f;
        public float hotTickInterval = 1f;
        public bool hotDivideAmount = true;

        [Header("Combat Rules")] 
        public bool allowInCombat = true;
        public bool slowInCombat = false;
        public float combatMultiplier = 0.5f;

        [Header("Cover Bonus")] 
        public bool coverBonus = false;
        public float coverMultiplier = 1.25f;

        [Header("Cooldown")] 
        public float cooldown = 2f;

        public override ConsumableType Type => ConsumableType.Heal;
        public override bool SupportsSmartTarget => supportsSmartTarget;
        public override float DoubleTapWindow => doubleTapWindow;

        public override bool CanUse(ItemUseContext ctx, InventorySlotRuntime slot)
        {
            var user = ctx.User;

            if (!allowInCombat && user.IsInCombat)
                return false;

            if (!user.Cooldowns.IsReady(GetCooldownKey(slot)))
                return false;

            if (user.Effects.HasAny<CastEffect>())
                return false;

            return true;
        }

        public override void Execute(ItemUseContext ctx, InventorySlotRuntime slot, Action onSuccess = null)
        {
            if (!CanUse(ctx, slot)) return;

            var target = ctx.User;

            float amount = healAmount;

            if (slowInCombat && ctx.User.IsInCombat)
                amount *= combatMultiplier;

            if (coverBonus && ctx.User.IsInCover)
                amount *= coverMultiplier;

            if (requiresCast)
            {
                ctx.User.Effects.Add(new CastEffect(
                    castTime,
                    () => Apply(ctx, target, slot, amount)
                ));
            }
            else
            {
                Apply(ctx, target, slot, amount);
            }
            
            onSuccess?.Invoke();

            // sound fx
            if(ctx.SpawnOrigin)
            {
                EventBus<AudioCueEvent>.Raise(
                    new AudioCueEvent(
                        ctx.SpawnOrigin.position,
                        audioConfig?.ToData()));
            }

#if UNITY_EDITOR
        DLog.Alert($"Heal applies to {ctx.User.DisplayName}");            
#endif
        }

        private void Apply(ItemUseContext ctx, IUnitRuntime target, InventorySlotRuntime slot, float amount)
        {
            if (isInstant && !isHoT)
                target.Stats.ModifyStat(StatId.Health, amount);

            if (isHoT)
            {
                ctx.User.Effects.Add(new HealOverTimeEffect(
                    target.Stats,
                    amount,
                    hotDuration,
                    hotTickInterval,
                    hotDivideAmount
                ));
            }

            if (cooldown > 0)
                ctx.User.Cooldowns.Set(GetCooldownKey(slot), cooldown);

            Consume(ctx, slot);
        }


        private string GetCooldownKey(InventorySlotRuntime slot) => slot.Item.Id.Guid;
    }

    // ConsumableType.cs
    public enum ConsumableType
    {
        None,
        Heal,
        Grenade,
        Booster,
        Antidote,
    }
}