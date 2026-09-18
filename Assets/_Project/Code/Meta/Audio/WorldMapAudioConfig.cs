using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Audio
{
    [CreateAssetMenu(
        fileName = "WorldMapAudioConfig",
        menuName = "Game Configs/Audio/UI/World Map Audio")]
    public sealed class WorldMapAudioConfig :
        ScriptableObject,
        IUIStyleConfig,
        IUIAudioConfig
    {
        [field: SerializeField] public string ConfigId { get; private set; }
        
        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }

        [Header("Panel")]
        public AudioCue showOverview;
        public AudioCue closeOverview;
        public AudioCue driveStart;
        public AudioCue driveFinish;
        
    }
}