
using Galactic1.Code.Gameplay.Audio.Voice;

namespace Galactic1.Code.Gameplay.Units.Definitions
{
    public abstract class UnitGameplayDefinition
    {
        /// <summary>Настройки сенсора (радиус, интервал, слух).</summary>
        public PerceptionDefinition Perception { get; }

        /// <summary>Параметры ближнего боя (урон, кулдаун, радиус).</summary>
        public MeleeCombatDefinition MeleeCombat { get; }
        
        public VoiceAudioConfig VoiceAudio { get; }

        protected UnitGameplayDefinition(
            PerceptionDefinition perception,
            MeleeCombatDefinition meleeCombat, 
            VoiceAudioConfig voiceAudio)
        {
            Perception = perception ?? throw new System.ArgumentNullException(nameof(perception));
            MeleeCombat = meleeCombat ?? throw new System.ArgumentNullException(nameof(meleeCombat));
            VoiceAudio = voiceAudio;
        }
    }
}