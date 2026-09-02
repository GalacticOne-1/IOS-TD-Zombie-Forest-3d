using Galactic1.Code.Gameplay.Audio.Voice;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    [CreateAssetMenu(
        fileName = "PlayerArchetypeConfig",
        menuName = "Game Configs/Recruitment/Player Archetype Config")]
    public sealed class PlayerArchetypeConfig : ScriptableObject
    {
        public PlayerBrainConfig Brain;
        public PlayerCombatConfig Combat;
        public PerceptionConfig Perception;
        public VoiceAudioConfig VoiceAudio;
        //public MovementConfig Movement;
        //public AbilityConfig Ability;
        //public EquipmentConfig Equipment;
    }
}