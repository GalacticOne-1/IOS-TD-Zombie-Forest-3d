using Galactic1.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Structs.UI
{
    [System.Serializable]
    public class InteractionIcons
    {
        [field: SerializeField] public Sprite defaultActionIcon {get; private set;}
        [field: SerializeField] private Sprite openChestIcon;
        [field: SerializeField] private Sprite miningResourceIcon;
        [field: SerializeField] private Sprite attackEnemyIcon;
        [field: SerializeField] private Sprite unlockSafeIcon;




    }
}