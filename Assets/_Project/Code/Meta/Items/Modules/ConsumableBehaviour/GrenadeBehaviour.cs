
using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Audio.Grenades;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Projectiles;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Gameplay;
using Galactic1.PoolObject;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Game.Meta.Items
{
    public class GrenadeBehaviour : ConsumableBehaviour
    {
        public override UseActivationType ActivationType => UseActivationType.Targeting;
        public override ConsumableType Type => ConsumableType.Grenade;


        [FormerlySerializedAs("audioDefinition")] [SerializeField] private GrenadeAudioConfig audioConfig;
 
        // ─────────────────────────────────────────────
        // Throw (без изменений)
        // ─────────────────────────────────────────────

        [Header("Throw")] 
        [SerializeField] private float throwSpeed = 0.6f;
        [SerializeField] private float throwHeight = 2.5f;
        [SerializeField] private GrenadeProjectile.DetonationMode detonationMode;
        [SerializeField] private float detonationDelay = 1;

        // ─────────────────────────────────────────────
        // Explosion — мгновенный взрыв (без изменений)
        // ─────────────────────────────────────────────

        [Header("Explosion")] [Tooltip("Немедленный урон от взрыва гранаты")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float outerExplosionRadius = 6f;
        [SerializeField] private float damage = 100f;
        [SerializeField] private float bigRadiusDamagePercent = 0.4f;
        [SerializeField] private AnimationCurve damageFalloff;



        // ─────────────────────────────────────────────
        // Temporal Zone — зональные гранаты
        // Оставить _zoneType = None → обычный взрыв (старое поведение)
        // ─────────────────────────────────────────────

        [Header("Temporal Zone")] [Tooltip("None = обычный взрыв. Остальные типы создают зону.")] 
        [SerializeField] private TemporalAoEType _zoneType = TemporalAoEType.None;
        [SerializeField] private AreaEffectConfig _zoneConfig;

        [Tooltip("Обязательный эффект! Импакт или луп для области")]
        [SerializeField] private VfxId vfxId;

        
        
        
        
        
        
        public float Damage => damage;
        public float ExplosionRadius => explosionRadius;
        public float OuterExplosionRadius => outerExplosionRadius;

        public AreaEffectConfig ZoneConfig => _zoneConfig;


        // ─────────────────────────────────────────────

        public override bool CanUse(ItemUseContext ctx, InventorySlotRuntime slot)
        {
            return true;
        }
        
        public override bool ValidateTarget(
            Vector3 origin,
            Vector3 target,
            UseModule config,
            out Vector3 projected)
        {
            projected = target;

            return Vector3.Distance(origin, target) <= config.Range;
        }

        public override void Execute(ItemUseContext ctx, InventorySlotRuntime slot, Action onSuccess = null)
        {
            var pool = ServiceLocator.Current.Get<PoolManager>();
            var grenade = pool.Get<GrenadeProjectile>(slot.Item);

            if (grenade == null)
                return;

            Consume(ctx, slot);

            Vector3 start = ctx.SpawnOrigin.position;
            Vector3 target = ctx.TargetPosition;

            var projectile = grenade.GetComponent<GrenadeProjectile>();

            if (_zoneType == TemporalAoEType.None)
            {
                // ── Мгновенный взрыв — старый путь, без изменений ──────────
                var aoe = new AoERequest
                {
                    Attacker = ctx.SceneUnit,
                    Position = target,
                    SmallRadius = explosionRadius,
                    BigRadius = outerExplosionRadius,
                    VfxId = vfxId,
                    MaxDamage = damage,
                    BigRadiusDamagePercent = bigRadiusDamagePercent,
                    DamageFalloff = damageFalloff,
                    Duration = 0f,
                    TargetMask = Layers.Damageable,
                    RequireLOS = true
                };

                projectile.Launch(
                    start, 
                    target, 
                    throwSpeed, 
                    throwHeight, 
                    aoe, 
                    detonationMode,
                    detonationDelay,
                    audioConfig);
            }
            else
            {
                // ── Зональная граната — новый путь ─────────────────────────
                // Position не задаём — GrenadeProjectile проставит из точки взрыва
                var temporal = new TemporalAoERequest
                {
                    Attacker = ctx.SceneUnit,
                    Radius = _zoneConfig.radius,
                    Type = _zoneType,
                    VfxId = vfxId,
                    VfxSelfDuration = _zoneConfig.vfxSelfDuration,
                    
                    Duration = _zoneConfig.duration,
                    DamagePerTick = _zoneConfig.damagePerTick,
                    TickInterval = _zoneConfig.tickInterval,
                    
                    SpeedMultiplier = _zoneConfig.speedMultiplier,
                    StunDuration = _zoneConfig.stunDuration,
                    TargetMask = Layers.Damageable
                };

                projectile.Launch(
                    start, 
                    target, 
                    throwSpeed, 
                    throwHeight, 
                    temporal,
                    detonationMode,
                    detonationDelay,
                    audioConfig);
            }
        }
    }
}