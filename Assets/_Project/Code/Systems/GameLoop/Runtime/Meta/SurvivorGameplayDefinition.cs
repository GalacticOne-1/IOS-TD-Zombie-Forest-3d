using Galactic1.Code.Gameplay.Audio.Voice;
using Galactic1.Code.Gameplay.Units.Definitions;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Gameplay configuration for a survivor unit.
    ///
    /// Exists in ALL contexts: camp, raid, meta preview.
    /// Does NOT contain inventory, stats, or equipment state.
    ///
    /// Built once per archetype (or per unit if brain settings are per-unit).
    /// Attached to RaidSurvivorDefinition.GameplayDefinition and to any
    /// future non-raid survivor runtime.
    /// </summary>
    public sealed class SurvivorGameplayDefinition : UnitGameplayDefinition
    {
        /// <summary>Auto-engage, cover, and re-engage thresholds for PlayerCommandBrain.</summary>
        public PlayerBrainDefinition BrainDefinition { get; }

        public SurvivorGameplayDefinition(
            PerceptionDefinition perception,
            MeleeCombatDefinition meleeCombat,
            PlayerBrainDefinition brainDefinition,
            VoiceAudioConfig voiceAudio)
            : base(perception, meleeCombat, voiceAudio)
        {
            BrainDefinition = brainDefinition
                            ?? throw new System.ArgumentNullException(nameof(brainDefinition));
        }
    }
}