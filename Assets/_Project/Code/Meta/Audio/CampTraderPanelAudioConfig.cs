using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Audio
{
    [CreateAssetMenu(
        fileName = "InventoryPanelAudio_",
        menuName = "Game Configs/Audio/UI/Camp Trader Panel Audio")]
    public sealed class CampTraderPanelAudioConfig :
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
        public AudioCue open;
        public AudioCue buy;
    }
    
}