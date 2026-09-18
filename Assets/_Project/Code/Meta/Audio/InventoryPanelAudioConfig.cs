using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Audio
{
    [CreateAssetMenu(
        fileName = "InventoryPanelAudioConfig",
        menuName = "Game Configs/Audio/UI/Inventory Panel Audio")]
    public sealed class InventoryPanelAudioConfig :
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

        [Header("Items")]
        public AudioCue itemDrag;
        public AudioCue itemDrop;
        public AudioCue itemUse;
        public AudioCue itemSplit;
        public AudioCue itemSort;
        public AudioCue itemRemove;
        
        [Header("Unit")]
        public AudioCue toSquad;
        public AudioCue fromSquad;
        public AudioCue equipmentAdd;
        public AudioCue equipmentRemove;
        public AudioCue unitBanish;
        
    }
}