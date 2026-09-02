using System;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Core.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Decides per-shot whether a tracer should be spawned, then raises
    /// VisualTracerEvent for each tracer pellet / bullet.
    ///
    /// Replaces WeaponView._tracerCounter / ShouldSpawnTracer() /
    /// RollNextTracerThreshold() / SpawnTracerProjectile().
    /// Those methods are deleted from WeaponView after this system is live.
    ///
    /// CADENCE (Aliens: Dark Descent scheme):
    ///   Non-shotgun: every TracerEveryNthShot shots, with ±1 randomness.
    ///     Shot, Shot, Shot, Shot+Tracer, Shot, Shot, Shot, Shot+Tracer ...
    ///   Shotgun: every shot is a tracer shot.
    ///     A subset of pellets (TracerPelletFraction) gets a tracer.
    ///     Selection is randomised via Fisher-Yates partial shuffle.
    ///
    /// TRACER DIRECTION REPLICATION:
    ///   SpreadAngles from FireRequest carry the per-pellet spread that was
    ///   already applied when the raycast ran. We apply the same angles to
    ///   Forward here so each tracer visually matches its real shot path.
    ///
    /// OUTPUT:
    ///   One VisualTracerEvent per tracer pellet/bullet.
    ///   FakeBulletSystem subscribes to VisualTracerEvent and spawns the
    ///   pooled projectile — it has no cadence logic of its own.
    ///
    /// Lifecycle:
    /// Created at raid init. Disposed at raid end (Dispose unsubscribes).
    /// Per-weapon state (_counter) is reset automatically because this is
    /// a raid-scoped singleton; weapons are re-equipped each raid.
    /// </summary>
    public sealed class TracerCadenceSystem
    {
        // Counter tracks shots-until-next-tracer for non-shotgun weapons.
        // Only one active weapon fires at a time in this game (squad-based),
        // so a single counter is sufficient. If simultaneous multi-weapon
        // fire becomes a requirement, key this by WeaponEntity or WeaponType.
        private int _counter;

        private readonly EventBinding<VisualShotEvent> _binding;

        public TracerCadenceSystem()
        {
            _counter = 0; // will be reset by RollNextThreshold on first shot
            _binding = new EventBinding<VisualShotEvent>(OnShot);
            EventBus<VisualShotEvent>.Register(_binding);
        }

        public void Dispose() => EventBus<VisualShotEvent>.Deregister(_binding);

        // ── Handler ──────────────────────────────────────────────────────────

        private void OnShot(VisualShotEvent e)
        {
            if (e.Weapon.TracerEveryNthShot <= 0) return; // tracers disabled for this weapon

            if (e.Weapon.WeaponType == WeaponType.Shotgun)
            {
                SpawnShotgunTracers(e);
            }
            else
            {
                SpawnRifleTracer(e);
            }
        }

        // ── Rifle / burst / semi-auto cadence ────────────────────────────────

        private void SpawnRifleTracer(VisualShotEvent e)
        {
            _counter--;
            if (_counter > 0) return;

            _counter = RollNextThreshold(e.Weapon.TracerEveryNthShot);

            // Single bullet — index 0.
            Vector3 dir = SpreadDirection(e.Forward, e.Request.SpreadAngles, 0);
            RaiseTracerEvent(e.Origin, dir, e.Weapon.WeaponType);
        }

        // ── Shotgun cadence ───────────────────────────────────────────────────

        private void SpawnShotgunTracers(VisualShotEvent e)
        {
            int total = Mathf.Max(1, e.Request.ProjectilesCount);
            int tracerCount = Mathf.Max(1, (int)(total * e.Weapon.TracerPelletFraction));
            
            // Fisher-Yates partial shuffle on a stackalloc buffer — zero heap alloc.
            Span<int> indices = stackalloc int[total];
            for (int i = 0; i < total; i++) indices[i] = i;

            for (int i = 0; i < tracerCount; i++)
            {
                int j = Random.Range(i, total);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            for (int i = 0; i < tracerCount; i++)
            {
                int pelletIndex = indices[i];
                Vector3 dir = SpreadDirection(e.Forward, e.Request.SpreadAngles, pelletIndex);
                RaiseTracerEvent(e.Origin, dir, e.Weapon.WeaponType);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Applies per-pellet spread angles from FireRequest to reproduce
        /// the exact shot direction the raycast used.
        /// </summary>
        private static Vector3 SpreadDirection(Vector3 forward, float[] spreadAngles, int pelletIndex)
        {
            if (spreadAngles == null || spreadAngles.Length < (pelletIndex + 1) * 2)
                return forward;

            float ax = spreadAngles[pelletIndex * 2];
            float ay = spreadAngles[pelletIndex * 2 + 1];
            return Quaternion.Euler(ax, ay, 0f) * forward;
        }

        private static void RaiseTracerEvent(Vector3 origin, Vector3 direction, WeaponType weaponType)
        {
            const float kVisualRange = 50f;
            EventBus<VisualTracerEvent>.Raise(new VisualTracerEvent(
                origin,
                origin + direction.normalized * kVisualRange,
                weaponType));
        }

        /// <summary>
        /// Randomises next tracer threshold: base ± 1, minimum 1.
        /// Mirrors the original WeaponView.RollNextTracerThreshold().
        /// </summary>
        private static int RollNextThreshold(int baseValue)
        {
            int result = baseValue + Random.Range(-1, 2); // Range(-1,2) → -1, 0, or +1
            return Mathf.Max(1, result);
        }
    }
}