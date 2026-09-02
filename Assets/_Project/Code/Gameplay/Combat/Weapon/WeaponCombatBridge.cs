using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Scene-layer bridge between weapon runtime (WeaponEntity/FireComponent)
    /// and the gameplay combat simulation (WeaponFireService).
    ///
    /// RESPONSIBILITY:
    /// - Subscribes to WeaponEntity.OnCombatFireRequested.
    /// - Owns muzzle Transform (origin/direction) — neither FireComponent
    ///   nor WeaponFireService know about Transforms.
    /// - Calls WeaponFireService.Execute() — gameplay resolves FIRST.
    /// - Raises VisualShotEvent AFTER gameplay resolution so visual systems
    ///   know a shot occurred (muzzle flash, tracer cadence).
    /// - Only after all of the above calls weapon.CompleteFire() which
    ///   triggers: cooldown (FireComponent) → NotifyFired (WeaponView visuals)
    ///   → RaiseShotLogicComplete (FSM unlock).
    ///
    /// SEQUENCING GUARANTEE (per trigger pull):
    ///   AE_DoShot
    ///     → FireComponent.OnAnimationFireEvent()
    ///       → entity.RaiseCombatFireRequested(request)
    ///         → WeaponCombatBridge.OnCombatFireRequested()
    ///             1. WeaponFireService.Execute()   [gameplay — authoritative]
    ///             2. EventBus<VisualShotEvent>     [visual shot notification]
    ///             3. weapon.CompleteFire()         [cooldown → visuals → FSM]
    ///
    /// CHANGE (Phase 3):
    /// Raises VisualShotEvent after gameplay resolution.
    /// MuzzleFlashSystem and TracerCadenceSystem subscribe to this event.
    /// WeaponView no longer spawns muzzle flash or tracer projectiles directly.
    ///
    /// One bridge instance per equipped weapon — created in WeaponView.Bind(),
    /// disposed in WeaponView.Unbind().
    /// </summary>
    public sealed class WeaponCombatBridge
    {
        private readonly WeaponEntity _weapon;
        private readonly IUnitSceneContext _attacker;
        private readonly Transform _muzzlePoint;
        private readonly WeaponFireService _fireService;

        public WeaponCombatBridge(
            WeaponEntity weapon,
            IUnitSceneContext attacker,
            Transform muzzlePoint,
            WeaponFireService fireService)
        {
            _weapon = weapon;
            _attacker = attacker;
            _muzzlePoint = muzzlePoint;
            _fireService = fireService;

            _weapon.OnCombatFireRequested += OnCombatFireRequested;
        }

        public void Dispose()
        {
            _weapon.OnCombatFireRequested -= OnCombatFireRequested;
        }

        // ── Core sequencing ──────────────────────────────────────────────────

        private void OnCombatFireRequested(FireRequest request)
        {
            Vector3 origin = _muzzlePoint.position;
            Vector3 direction = (request.TargetAimPoint - _muzzlePoint.position).normalized;

            // ── 1. Gameplay resolution — MUST happen before any visuals ──────
            _fireService.Execute(_weapon, request, _attacker, origin, direction);
            
            // ── 2. Gunshot audio notification ─────────────────────────────────
            // Raised once per fire action, regardless of pellet count. Silent
            // weapons (no WeaponAudioDefinition assigned) never raise this.
            RaiseAudioGunshotEvent(origin);

            // ── 3. Visual shot notification ───────────────────────────────────
            // Raised after gameplay so visual systems can react to a confirmed
            // fire action. Muzzle flash and tracer cadence live here, not in
            // WeaponView, so they are decoupled from the scene-layer component.
            RaiseVisualShotEvent(request, origin);

            // ── 4. Weapon lifecycle completion ────────────────────────────────
            // Order inside CompleteFire:
            //   a) FireComponent.CompleteFire  — applies cooldown
            //   b) NotifyFired                 — WeaponView: animation/state
            //   c) RaiseShotLogicComplete      — EngagingState FSM unlocks
            _weapon.CompleteFire(request);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        
        private void RaiseAudioGunshotEvent(Vector3 origin)
        {
            var audio = _weapon.Definition.Audio;
            if (audio == null)
                return;

            EventBus<WeaponAudioEvent>.Raise(new WeaponAudioEvent(
                origin,
                audio,
                WeaponAudioEventType.Fire));
        }

        private void RaiseVisualShotEvent(FireRequest request, Vector3 origin)
        {
            Vector3 aimDirection = (request.TargetAimPoint - origin).normalized;
            
            EventBus<VisualShotEvent>.Raise(new VisualShotEvent(
                _attacker,
                origin,
                aimDirection,   // _muzzlePoint.forward
                _muzzlePoint.rotation,
                request,
                _weapon.Definition));
        }
    }
}