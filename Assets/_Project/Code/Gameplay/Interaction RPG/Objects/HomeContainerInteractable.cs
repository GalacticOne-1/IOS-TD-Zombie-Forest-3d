
using Galactic1.Systems.Inventory;
using Galactic1.UI;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    /// <summary>
    /// Ящик с лутом.
    /// </summary>
    public class HomeContainerInteractable : InteractableBase, IActionInteractable
    {
        
        public override ActionType ActionType => ActionType.OpenContainer;

        
        public override void Interact(Transform interactor)
        {
            Debug.Log("[Chest] Opened: " + name);
            
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory,
                    //     GetComponent<HomeContainer>());
                });
        }

        public override InteractionInfo GetInfo()
        {
            return new InteractionInfo { Name = "Chest", Icon = null, IsAvailable = IsAvailable };
        }

        public override void OnFocus()
        {
            base.OnFocus();
            // дополнительные эффекты
        }

        public override void OnFocusLost()
        {
            base.OnFocusLost();
        }
    }
}