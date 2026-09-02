using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Grenades;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Combat.Visual;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Per-raid combat subsystem container.
    ///
    /// Holds all gameplay combat services and the visual runtime.
    /// Created in RaidInProgressState.BuildCombatRuntime().
    /// Disposed when the raid ends (SUB_RaidCleanupState or RaidInProgressState.Exit).
    ///
    /// CHANGE (Phase 3):
    /// CombatVisualRuntime added — owns Router, impact/decal/tracer/muzzle systems.
    /// Dispose() now tears down visual systems alongside gameplay services.
    /// </summary>
    public sealed class CombatRuntime
    {
        // ── Gameplay ──────────────────────────────────────────────────────────
        public readonly WeaponFireService WeaponFireService;
        public readonly CombatBatchProcessor BatchProcessor;
        public readonly Suppression.SuppressionSystem SuppressionSystem;

        // ── Visual ────────────────────────────────────────────────────────────
        public readonly CombatVisualRuntime Visual;
        public readonly WeaponAudioSystem WeaponAudioSystem;
        private readonly GrenadeAudioPlaybackSystem GrenadeAudioSystem;
        private readonly AudioCueSystem AudioCueSystem;
        private readonly VoiceAudioSystem VoiceAudioSystem;

        // ── Dev ───────────────────────────────────────────────────────────────
        public readonly CombatDebugDrawer DebugDrawer;

        public CombatRuntime(
            WeaponFireService weaponFireService,
            CombatBatchProcessor batchProcessor,
            Suppression.SuppressionSystem suppressionSystem,
            CombatVisualRuntime visual,
            WeaponAudioSystem weaponAudioSystem,
            GrenadeAudioPlaybackSystem grenadeAudioSystem,
            AudioCueSystem audioCueSystem,
            VoiceAudioSystem voiceAudioSystem,
            CombatDebugDrawer debugDrawer)
        {
            WeaponFireService = weaponFireService;
            BatchProcessor = batchProcessor;
            SuppressionSystem = suppressionSystem;
            Visual = visual;
            WeaponAudioSystem = weaponAudioSystem;
            GrenadeAudioSystem = grenadeAudioSystem;
            AudioCueSystem = audioCueSystem;
            VoiceAudioSystem = voiceAudioSystem;
            DebugDrawer = debugDrawer;
        }

        public void Dispose()
        {
            Visual?.Dispose();
            WeaponAudioSystem?.Dispose();
            GrenadeAudioSystem?.Dispose();
            AudioCueSystem?.Dispose();
            VoiceAudioSystem?.Dispose();
            // GameplaySuppressionEvent bindings are cleared by EventBus<T>.Clear()
            // on SceneServicesClearEvent — no explicit unsubscribe needed here.
        }
    }
}