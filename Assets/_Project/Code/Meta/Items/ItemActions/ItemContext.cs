
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Items
{
    public struct ItemContext
    {
        public InventoryManagementWindow window {get; private set;}
        public InventoryView view {get; private set;}
        public IInventorySource inventorySource {get; private set;}
        public InventorySlotRuntime slot {get; private set;}
        public int slotIndex {get; private set;}
        
        

        public ItemContext(
            IInventorySource source, 
            InventorySlotRuntime slot, 
            int slotIndex, 
            InventoryManagementWindow window = null, 
            InventoryView view = null)
        {
            inventorySource = source;
            this.slot = slot;
            this.slotIndex = slotIndex;
            this.window = window;
            this.view = view;
            
        }
    }
}