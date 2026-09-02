
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Test
{
    public class TestMenu : MonoBehaviour
    {
        public void OpenPlayer()
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory,
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerEquipment);
                });
            
        }

        public void OpenCrate()
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory,
                    //     FindAnyObjectByType<HomeContainer>());
                });
        }

        public void OpenDragon()
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().DragonInventory,
                    //     ServiceLocator.Current.Get<InventoryRepository>().DragonEquipment);
                });
        }

        public void OpenPlayerDragon()
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory,
                    //     ServiceLocator.Current.Get<InventoryRepository>().DragonInventory);
                });
        }
    }
}