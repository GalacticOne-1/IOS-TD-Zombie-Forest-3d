using Galactic1.Gameplay.Interaction;
using Galactic1.Gameplay.Interaction.Objects;
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




        public Sprite GetIconFor(IInteractable interactable)
        {
            return interactable switch
            {
                HomeContainerInteractable or ContainerInteractable => openChestIcon,
                ResourceNodeBase => miningResourceIcon,
                //Corpse => lootCorpseIcon,
                EnemyInteractable => attackEnemyIcon,
                //Safe => unlockSafeIcon,
                _ => defaultActionIcon
            };
        }
    }
}