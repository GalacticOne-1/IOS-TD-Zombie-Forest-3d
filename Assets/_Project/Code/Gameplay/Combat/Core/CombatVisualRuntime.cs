using System.Collections.Generic;
using Galactic1.Code.Data.Combat;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Owns and wires all per-raid combat visual systems.
    ///
    /// Created by RaidInProgressState.BuildCombatRuntime() alongside the
    /// gameplay combat stack. Disposed when the raid ends.
    ///
    /// Systems owned:
    ///   CombatEventRouter      — bridges gameplay events to visual/audio events
    ///   ImpactAggregationSystem — VisualImpactEvent → impact particle FX
    ///   DecalSystem            — VisualImpactEvent → bullet hole / blood decals
    ///   MuzzleFlashSystem      — VisualShotEvent → muzzle flash FX
    ///   TracerCadenceSystem    — VisualShotEvent → VisualTracerEvent (cadence)
    ///   FakeBulletSystem       — VisualTracerEvent → pooled TracerProjectile
    ///   AsyncFXSpawnQueue      — frame-budgeted FX dispatch
    ///   CombatFXLODService     — distance-based FX culling
    ///
    /// USAGE in RaidInProgressState.BuildCombatRuntime():
    ///
    ///   var visualRuntime = new CombatVisualRuntime(
    ///       surfaceFXConfigs,
    ///       effectSystem,
    ///       muzzleFxId,
    ///       Camera.main,
    ///       maxFxPerFrame: 8);
    ///
    ///   return new CombatRuntime(weaponFireService, ..., visualRuntime);
    ///
    /// Call Dispose() from CombatRuntime.Dispose() at raid end.
    /// </summary>
    public sealed class CombatVisualRuntime
    {
        // ── Public for CombatRuntime / debug access ───────────────────────────
        public readonly CombatEventRouter Router;
        public readonly AsyncFXSpawnQueue FXQueue;
        public readonly CombatFXLODService LOD;

        // ── Private owned systems ─────────────────────────────────────────────
        private readonly ImpactSurfaceResolver _impactSystem;
        private readonly DecalSystem _decalSystem;
        private readonly TracerCadenceSystem _tracerCadence;
        private readonly FakeBulletSystem _fakeBullets;

        /// <param name="surfaceFXConfigs">
        /// Configs from a CombatSurfaceFXConfig SO collection.
        /// Used to build CombatSurfaceFXDatabase for impact / decal systems.
        /// </param>
        /// <param name="effectSystem">
        /// Shared EffectRequestSystem from ServiceLocator.
        /// Drives all pooled VFX spawning.
        /// </param>
        /// <param name="muzzleFxId">
        /// VfxId registered in EffectRequestSystem for the muzzle flash particle.
        /// Pass null to disable muzzle flash.
        /// </param>
        /// <param name="camera">Main camera for LOD distance checks.</param>
        /// <param name="maxFxPerFrame">
        /// Max FX spawned per LateUpdate tick (AsyncFXSpawnQueue budget).
        /// </param>
        public CombatVisualRuntime(
            CombatSurfaceFXDatabase surfaceDB,
            CombatTracerDatabase tracerDB,
            EffectRequestSystem effectSystem,
            Camera camera,
            int maxFxPerFrame = 8)
        {
            // ── Infrastructure ────────────────────────────────────────────────
            LOD = new CombatFXLODService(camera);
            FXQueue = new AsyncFXSpawnQueue(maxFxPerFrame, effectSystem);
            FXQueue.Initialize();


            // ── Event routing ─────────────────────────────────────────────────
            Router = new CombatEventRouter();

            // ── Impact / decal ────────────────────────────────────────────────
            _impactSystem = new ImpactSurfaceResolver(LOD, FXQueue, surfaceDB);
            _decalSystem = new DecalSystem(LOD, FXQueue, surfaceDB);

            // ── Shot visual pipeline ──────────────────────────────────────────
            _tracerCadence = new TracerCadenceSystem();
            _fakeBullets = new FakeBulletSystem(LOD, tracerDB);
        }

        /// <summary>
        /// Unsubscribes all systems from EventBus and releases the FX queue.
        /// Call from CombatRuntime.Dispose() at raid end.
        /// </summary>
        public void Dispose()
        {
            Router.Dispose();
            _impactSystem.Dispose();
            _decalSystem.Dispose();
            _tracerCadence.Dispose();
            _fakeBullets.Dispose();
            FXQueue.Dispose();
        }
    }
}