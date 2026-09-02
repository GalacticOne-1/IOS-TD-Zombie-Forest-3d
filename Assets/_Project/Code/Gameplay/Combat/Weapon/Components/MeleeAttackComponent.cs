using System;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    /// <summary>
    /// Melee attack execution component.
    ///
    /// Hit origin resolution (two paths, one component):
    ///
    ///   PATH A — Weapon-based melee (SurvivorInstance with WeaponRigController):
    ///     hitOriginOverride Transform is provided at construction.
    ///     OverlapSphere uses that Transform.position directly — exact rig bone.
    ///     HitOriginOffset from definition is ignored (zero anyway).
    ///
    ///   PATH B — Body-based melee (ZombieInstance without weapon rig):
    ///     hitOriginOverride is null.
    ///     OverlapSphere center = unitRoot.position
    ///                           + unitRoot.forward * HitOriginOffset.z
    ///                           + Vector3.up        * HitOriginOffset.y
    ///     This moves the sphere from root (feet) to the actual contact zone
    ///     (chest height, slightly forward) without requiring a scene Transform.
    ///
    /// Why not always use a Transform:
    ///   ZombieInstance has no weapon rig. The fallback was root transform,
    ///   which centers the hit sphere at foot level — causing misses on upright targets.
    ///   The offset approach is scene-independent, definition-driven, and stable.
    /// </summary>
    public sealed class MeleeAttackComponent : IMeleeAnimationReceiver
    {
        public float AttackRange { get; }
        public float HitRange { get; }
        public Vector3 HitOriginOffset { get; }
        public float Damage { get; }
        public float Cooldown { get; }
        public float ReadyToAttackAngle { get; }

        private float _cooldownRemaining;
        public bool IsReady => _cooldownRemaining <= 0f;

        private CombatEventService _combatEvents;
        private readonly LayerMask _enemyLayer;
        private readonly IUnitSceneContext _unitAdapter;
        private readonly Transform _unitRoot;
        private readonly Transform _hitOriginOverride; // null for zombies

        public event Action OnHitApplied;
        public event Action OnHitLogicComplete;
        public event Action OnAttackAnimationRequested;

        /// <summary>
        /// Full constructor — used by ZombieInstance (no weapon rig).
        /// Hit sphere center is computed from unitRoot + definition.HitOriginOffset.
        /// </summary>
        public MeleeAttackComponent(
            IUnitSceneContext unitAdapter,
            Transform unitRoot,
            LayerMask enemyLayer,
            MeleeCombatDefinition definition)
            : this(unitAdapter, unitRoot, null, enemyLayer, definition)
        {
        }

        /// <summary>
        /// Override constructor — used by SurvivorInstance with WeaponRigController.
        /// Hit sphere center is hitOriginOverride.position (exact rig bone).
        /// </summary>
        public MeleeAttackComponent(
            IUnitSceneContext unitAdapter,
            Transform unitRoot,
            Transform hitOriginOverride,
            LayerMask enemyLayer,
            MeleeCombatDefinition definition)
        {
            _unitAdapter = unitAdapter;
            _unitRoot = unitRoot;
            _hitOriginOverride = hitOriginOverride;
            _enemyLayer = enemyLayer;

            AttackRange = definition.AttackRange;
            HitRange = definition.HitRange;
            HitOriginOffset = definition.HitOriginOffset;
            Damage = definition.Damage;
            Cooldown = definition.Cooldown;
            ReadyToAttackAngle = definition.ReadyToAttackAngle;
        }

        public void Tick(float dt)
        {
            if (_cooldownRemaining > 0f) _cooldownRemaining -= dt;
        }

        public void Execute()
        {
            if (!IsReady) return;
            _cooldownRemaining = Cooldown;
            OnAttackAnimationRequested?.Invoke();
        }

        public void OnAnimationMeleeHitEvent()
        {
            Vector3 sphereCenter = ResolveHitOrigin();

            var hits = Physics.OverlapSphere(sphereCenter, HitRange, _enemyLayer);

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<HitboxProxy>(out var proxy)) continue;

                var receiver = proxy.Receiver;
                if (!TeamService.CanDamage(_unitAdapter.RuntimeBase, receiver.Unit?.RuntimeBase))
                    continue;

                var hitInfo = new HitInfo
                {
                    Point = sphereCenter,
                    Normal = Vector3.up,
                    Collider = hit,
                    Transform = hit.transform,
                };

                var result = DamageResolver.Apply(receiver, _unitAdapter, Damage, DamageType.Hit, hitInfo);

                _combatEvents ??= ServiceLocator.Current.Get<CombatEventService>();
                _combatEvents.RaiseHit(
                    _unitAdapter,
                    receiver.Unit,
                    result,
                    hitInfo,
                    shotDirection: Vector3.zero);
            }

            OnHitApplied?.Invoke();
        }

        public void OnAnimationFinished()
        {
            OnHitLogicComplete?.Invoke();
        }

        public void Reset() => _cooldownRemaining = 0f;

        // ── Hit origin resolution ─────────────────────────────────────────

        /// <summary>
        /// PATH A: rig-driven (weapon socket bone) — exact, used for survivors.
        /// PATH B: offset-driven (root + forward/up offset) — used for zombies.
        ///
        /// The offset moves the sphere from foot level to the contact zone:
        ///   forward * z  = pushes sphere in front of the unit
        ///   up * y       = lifts sphere to torso/arm height
        /// </summary>
        public Vector3 ResolveHitOrigin()
        {
            if (_hitOriginOverride != null)
                return _hitOriginOverride.position;

            if (_unitRoot == null)
                return Vector3.zero;

            return _unitRoot.position
                   + _unitRoot.forward * HitOriginOffset.z
                   + Vector3.up * HitOriginOffset.y;
        }
    }
}