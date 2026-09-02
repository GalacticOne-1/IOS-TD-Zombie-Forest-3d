
using Galactic1.Core.Enums;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Bridges authoritative gameplay events into visual and audio channels.
    ///
    /// RULE:
    /// Gameplay raises GameplayHitEvent / GameplayDeathEvent / CombatMissEvent.
    /// Router re-raises VisualImpactEvent / AudioImpactEvent / AudioVoiceEvent etc.
    /// Visual and audio systems NEVER subscribe to gameplay events directly.
    ///
    /// This keeps the dependency graph clean:
    ///   Gameplay → Router → Visual / Audio
    ///
    /// CHANGE (Phase 3):
    /// - GameplayHitEvent now carries Normal — Vector3.up workaround removed.
    /// - CombatMissEvent subscription added: missed shots also produce
    ///   a VisualTracerEvent so FakeBulletSystem can render miss tracers.
    ///   (Miss tracers use a single straight ray along the shot direction.)
    ///
    /// NOTE: VisualShotEvent (muzzle flash + tracer cadence) is raised by
    /// WeaponCombatBridge, NOT by this Router. Muzzle flash is tied to the
    /// fact of firing, not to hit/miss, so it belongs at the shot origin.
    ///
    /// Lifecycle:
    /// Created at raid init (BuildCombatRuntime).
    /// Disposed at raid end via Dispose() to unsubscribe from EventBus.
    /// </summary>
    public sealed class CombatEventRouter
    {
        private readonly EventBinding<CombatHitEvent> _hitBinding;
        private readonly EventBinding<CombatDeathEvent> _deathBinding;
        private readonly EventBinding<CombatMissEvent> _missBinding;

        public CombatEventRouter()
        {
            _hitBinding = new EventBinding<CombatHitEvent>(OnGameplayHit);
            _deathBinding = new EventBinding<CombatDeathEvent>(OnGameplayDeath);
            _missBinding = new EventBinding<CombatMissEvent>(OnCombatMiss);

            EventBus<CombatHitEvent>.Register(_hitBinding);
            EventBus<CombatDeathEvent>.Register(_deathBinding);
            EventBus<CombatMissEvent>.Register(_missBinding);
        }

        public void Dispose()
        {
            EventBus<CombatHitEvent>.Deregister(_hitBinding);
            EventBus<CombatDeathEvent>.Deregister(_deathBinding);
            EventBus<CombatMissEvent>.Deregister(_missBinding);
        }

        // ── Gameplay → Visual + Audio ────────────────────────────────────────

        private void OnGameplayHit(CombatHitEvent e)
        {
            // Visual — impact FX and decals.
            // Normal now comes from the real raycast result via GameplayHitEvent.Normal.
            EventBus<VisualImpactEvent>.Raise(new VisualImpactEvent(
                e.Point,
                e.Normal, 
                e.ShotDirection,
                e.Surface));

            // Audio — delayed impact sound after gunshot.
            EventBus<AudioImpactEvent>.Raise(new AudioImpactEvent(
                e.Point,
                e.Surface,
                delay: 0.05f));

            // Audio — unit voice reaction on damage.
            if (e.Target != null)
            {
                var voice = e.Target.RuntimeBase.RuntimeDefinition.VoiceAudio.ToData();

                if (voice == null)
                    return;

                EventBus<AudioVoiceEvent>.Raise(
                    new AudioVoiceEvent(
                        e.Point,
                        voice,
                        VoiceEventType.Damage,
                        priority: 30));
            }
        }

        private void OnGameplayDeath(CombatDeathEvent e)
        {
            // Louder voice on death — higher priority.
            if (e.Victim != null)
            {
                var voice = e.Victim.RuntimeBase.RuntimeDefinition.VoiceAudio.ToData();

                if (voice == null)
                    return;
                
                EventBus<AudioVoiceEvent>.Raise(new AudioVoiceEvent(
                    e.Point,
                    voice,
                    VoiceEventType.Death,
                    priority: 80));
            }
        }

        /// <summary>
        /// Miss path: no impact FX, but we still produce a VisualTracerEvent
        /// so FakeBulletSystem can render the tracer flying into empty space.
        ///
        /// NOTE: TracerCadenceSystem owns the cadence decision for hits.
        /// For misses we bypass cadence — a missed shot always shows a tracer
        /// so the player can read where bullets are going.
        /// If miss-tracer spamming becomes a problem, a separate
        /// miss-tracer LOD/cadence can be added here later.
        /// </summary>
        private void OnCombatMiss(CombatMissEvent e)
        {
            // VisualTracerEvent for a miss: single ray, full direction vector.
            // Start = muzzle origin, End = as far as weapon range goes.
            // FakeBulletSystem does not need ShotsCount or SpreadAngles here —
            // the direction is already the final spread-applied shot vector
            // (set in BurstFireResolver from FireRequest.SpreadAngles).
            // EventBus<VisualTracerEvent>.Raise(new VisualTracerEvent(
            //     e.Origin,
            //     e.Origin + e.Direction.normalized * 50f,
            //     e.WeaponType)); // fallback — miss не знает тип оружия
        }
    }
}