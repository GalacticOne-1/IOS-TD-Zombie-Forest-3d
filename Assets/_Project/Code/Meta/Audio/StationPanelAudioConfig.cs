using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Audio
{
    [CreateAssetMenu(
        fileName = "StationPanelAudioConfig",
        menuName = "Game Configs/Audio/UI/Station Panel Audio Config")]
    public sealed class StationPanelAudioConfig :
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

        
        public AudioCue action;

        
        
    }
}