using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    /// <summary>
    /// Scene-layer weapon component.
    ///
    /// RESPONSIBILITIES (Phase 3 — final):
    /// - Bind / Unbind weapon runtime to the scene object.
    /// - Expose MuzzlePoint (Transform) for WeaponCombatBridge and camera systems.
    /// - Forward Animation Events (AE_*) to the weapon entity.
    /// - React to WeaponState changes for animation controller integration.
    ///
    /// NOT RESPONSIBLE FOR (removed in Phase 3):
    /// - Muzzle flash        → MuzzleFlashSystem (subscribes to VisualShotEvent)
    /// - Tracer spawning     → TracerCadenceSystem + FakeBulletSystem
    /// - Tracer cadence      → TracerCadenceSystem
    /// - BaseProjectile pool → FakeBulletSystem
    /// - Any combat FX       → Visual Systems layer
    ///
    /// The full combat visual pipeline is now:
    ///   WeaponCombatBridge raises VisualShotEvent
    ///     → MuzzleFlashSystem spawns muzzle FX via AsyncFXSpawnQueue
    ///     → TracerCadenceSystem decides tracer → raises VisualTracerEvent
    ///     → FakeBulletSystem spawns pooled TracerProjectile
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        [Header("Mount Points")] [SerializeField]
        private Transform muzzlePoint;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem muzzleFlash;

        // ── Runtime references ────────────────────────────────────────────────

        private ISceneUnit _unitRuntime;
        private WeaponEntity _weaponEntity;
        private WeaponDefinitionData _definition;
        private WeaponCombatBridge _combatBridge;

        // ── Public accessors ──────────────────────────────────────────────────

        public Transform MuzzlePoint => muzzlePoint;
        
        private EventBinding<VisualShotEvent> _shotBinding;
        
        

        // ── Bind / Unbind ─────────────────────────────────────────────────────

        public void Bind(
            ISceneUnit sceneUnit,
            WeaponEntity entity,
            WeaponDefinitionData weaponDef)
        {
            _unitRuntime = sceneUnit;
            _weaponEntity = entity;
            _definition = weaponDef;

            _weaponEntity.OnFired += OnFired;
            _weaponEntity.OnStateChanged += OnStateChanged;
            _weaponEntity.OnReloadCompleted += OnReloadClips;
            
            _shotBinding = new EventBinding<VisualShotEvent>(OnVisualShot);
            EventBus<VisualShotEvent>.Register(_shotBinding);

            var gameLoopContext = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            if (gameLoopContext.CurrentRaid != null)
            {
                _combatBridge = new WeaponCombatBridge(
                    entity,
                    sceneUnit,
                    muzzlePoint,
                    gameLoopContext.CurrentRaid.Combat.WeaponFireService);
            }
        }

        public void Unbind()
        {
            if (_weaponEntity == null) return;

            _weaponEntity.OnFired -= OnFired;
            _weaponEntity.OnStateChanged -= OnStateChanged;
            _weaponEntity.OnReloadCompleted -= OnReloadClips;
            
            EventBus<VisualShotEvent>.Deregister(_shotBinding);
            _shotBinding = null;

            _combatBridge?.Dispose();
            _combatBridge = null;
            _weaponEntity = null;
        }

        // ── Animation Events ──────────────────────────────────────────────────

        /// <summary>
        /// Called by Unity Animation system at the keyframe the casing ejects.
        /// Forwards to the weapon entity so WeaponAudioPlayer or FX systems
        /// can react via events without polling.
        /// </summary>
        public void AE_EjectCasing()
        {
            // Spawn casing FX here if needed, or raise a VisualCasingEvent.
            // Left as a hook — casing visual is not part of Phase 3 scope.
        }

        // ── Callbacks ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called AFTER WeaponFireService has fully resolved gameplay AND
        /// WeaponCombatBridge has raised VisualShotEvent.
        ///
        /// At this point muzzle flash and tracer are already queued by their
        /// respective systems. OnFired here is only for WeaponView-local concerns
        /// such as animation state updates.
        /// </summary>
        private void OnFired(FireRequest request)
        {
            // Animation-only reactions go here if needed.
            // Combat FX are handled by MuzzleFlashSystem and FakeBulletSystem.
            
            // === реализовано в WeaponCombatBridge
        }

        private void OnStateChanged(WeaponState state)
        {
            // e.g. play empty-clip sound via WeaponAudioPlayer (Phase 5)
        }

        private void OnReloadClips()
        {
            EventBus<WeaponAudioEvent>.Raise(new WeaponAudioEvent(
                transform.position,
                _definition.Audio,
                WeaponAudioEventType.ReloadComplete));
        }

        // т.е событие выстрела глобальное, фильтруем по стрелку
        private void OnVisualShot(VisualShotEvent e)
        {
            if (e.Attacker == _unitRuntime)
            {
                SpawnMuzzleFlash();
            }
        }

        
        
        
        private void SpawnMuzzleFlash()
        {
            if (muzzleFlash == null) 
                return;
            
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Clear();
            muzzleFlash.Play();
        }

        // ── Editor ────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (muzzlePoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(muzzlePoint.position, 0.04f);
                Gizmos.DrawRay(muzzlePoint.position, muzzlePoint.forward * 0.4f);
            }
        }
#endif
    }
}